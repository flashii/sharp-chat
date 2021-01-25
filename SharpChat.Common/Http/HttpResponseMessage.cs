﻿using SharpChat.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SharpChat.Http {
    public class HttpResponseMessage : HttpMessage {
        public override string ProtocolVersion { get; }
        public int StatusCode { get; }
        public string StatusMessage { get; }

        public override IEnumerable<HttpHeader> Headers { get; }

        public override Stream Body { get; }

        public string Connection
            => Headers.FirstOrDefault(x => x.Name == HttpConnectionHeader.NAME)?.Value.ToString() ?? string.Empty;
        public string Server
            => Headers.FirstOrDefault(x => x.Name == HttpServerHeader.NAME)?.Value.ToString() ?? string.Empty;
        public DateTimeOffset Date
            => Headers.Where(x => x.Name == HttpDateHeader.NAME).Cast<HttpDateHeader>().FirstOrDefault()?.DateTime ?? DateTimeOffset.MinValue;
        public HttpMediaType ContentType
            => Headers.Where(x => x.Name == HttpContentTypeHeader.NAME).Cast<HttpContentTypeHeader>().FirstOrDefault()?.MediaType
            ?? HttpMediaType.OctetStream;
        public IEnumerable<string> ContentEncodings
            => Headers.Where(x => x.Name == HttpContentEncodingHeader.NAME).Cast<HttpContentEncodingHeader>().FirstOrDefault()?.Encodings
            ?? Enumerable.Empty<string>();
        public IEnumerable<string> TransferEncodings
            => Headers.Where(x => x.Name == HttpTransferEncodingHeader.NAME).Cast<HttpTransferEncodingHeader>().FirstOrDefault()?.Encodings
            ?? Enumerable.Empty<string>();

        public HttpResponseMessage(
            int statusCode, string statusMessage, string protocolVersion,
            IEnumerable<HttpHeader> headers, Stream body
        ) {
            ProtocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
            StatusCode = statusCode;
            StatusMessage = statusMessage ?? string.Empty;
            Headers = (headers ?? throw new ArgumentNullException(nameof(headers))).ToArray();
            Body = body;
        }

        // there's probably a less stupid way to do this, be my guest and call me an idiot
        private static void ProcessEncoding(Stack<string> encodings, Stream stream, bool transfer) {
            using MemoryStream temp = new MemoryStream();
            bool inTemp = false;

            while(encodings.TryPop(out string encoding)) {
                Stream target = (inTemp = !inTemp) ? temp : stream,
                    source = inTemp ? stream : temp;

                target.SetLength(0);
                source.Seek(0, SeekOrigin.Begin);

                switch(encoding) {
                    case HttpEncoding.GZIP:
                    case HttpEncoding.XGZIP:
                        using(GZipStream gzs = new GZipStream(source, CompressionMode.Decompress, true))
                            gzs.CopyTo(target);
                        break;

                    case HttpEncoding.DEFLATE:
                        using(DeflateStream def = new DeflateStream(source, CompressionMode.Decompress, true))
                            def.CopyTo(target);
                        break;

                    case HttpEncoding.BROTLI:
                        if(transfer)
                            goto default;
                        using(BrotliStream br = new BrotliStream(source, CompressionMode.Decompress, true))
                            br.CopyTo(target);
                        break;

                    case HttpEncoding.IDENTITY:
                        break;

                    case HttpEncoding.CHUNKED:
                        if(!transfer)
                            goto default;
                        throw new IOException(@"Invalid use of chunked encoding type in Transfer-Encoding header.");

                    default:
                        throw new IOException(@"Unsupported encoding supplied.");
                }
            }

            if(inTemp) {
                stream.SetLength(0);
                temp.Seek(0, SeekOrigin.Begin);
                temp.CopyTo(stream);
            }
        }

        public static HttpResponseMessage ReadFrom(Stream stream) {
            // ignore this function, it doesn't exist
            string readLine() {
                const byte cr = 13, lf = 10;
                StringBuilder sb = new StringBuilder();
                int byt;
                bool gotCR = false;

                for(; ; ) {
                    byt = stream.ReadByte();
                    if(byt == -1 && sb.Length == 0)
                        return null;

                    if(gotCR) {
                        if(byt == lf)
                            break;
                        sb.Append('\r');
                    }

                    gotCR = byt == cr;
                    if(!gotCR)
                        sb.Append((char)byt);
                }

                return sb.ToString();
            }
            
            long contentLength = -1;
            Stack<string> transferEncodings = null;
            Stack<string> contentEncodings = null;

            // Read initial header
            string line = readLine();
            if(line == null)
                throw new IOException(@"Failed to read initial HTTP header.");
            if(!line.StartsWith(@"HTTP/"))
                throw new IOException(@"Response is not a valid HTTP message.");
            string[] parts = line[5..].Split(' ', 3);
            if(!int.TryParse(parts.ElementAtOrDefault(1), out int statusCode))
                throw new IOException(@"Invalid HTTP status code format.");
            string protocolVersion = parts.ElementAtOrDefault(0);
            string statusMessage = parts.ElementAtOrDefault(2);

            // Read header key-value pairs
            List<HttpHeader> headers = new List<HttpHeader>();

            while((line = readLine()) != null) {
                if(string.IsNullOrWhiteSpace(line))
                    break;

                parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
                if(parts.Length < 2)
                    throw new IOException(@"Invalid HTTP header in response.");

                string hName = HttpHeader.NormaliseName(parts.ElementAtOrDefault(0) ?? string.Empty),
                    hValue = parts.ElementAtOrDefault(1);
                if(string.IsNullOrEmpty(hName))
                    throw new IOException(@"Invalid HTTP header name.");

                HttpHeader header = HttpHeader.Create(hName, hValue);

                Console.ForegroundColor = ConsoleColor.Green;
                if(header is HttpContentLengthHeader hclh)
                    contentLength = (long)hclh.Value;
                else if(header is HttpTransferEncodingHeader hteh)
                    transferEncodings = new Stack<string>(hteh.Encodings);
                else if(header is HttpContentEncodingHeader hceh)
                    contentEncodings = new Stack<string>(hceh.Encodings);
                else if(header is HttpCustomHeader)
                    Console.ForegroundColor = ConsoleColor.Red;

                Logger.Debug(header);
                Console.ResetColor();

                headers.Add(header);
            }

            Stream body = null;

            void readBuffer(long length = -1) {
                int bufferSize = 8192;
                byte[] buffer = new byte[bufferSize];
                int read;
                long remaining = length;

                while((read = stream.Read(buffer, 0, bufferSize)) > 0) {
                    body.Write(buffer, 0, read);

                    if(length >= 0) {
                        remaining -= read;
                        if(remaining < 1)
                            break;
                        if(bufferSize > remaining)
                            bufferSize = (int)remaining;
                    }
                }
            }

            // Read body
            if(transferEncodings != null && transferEncodings.Any() && transferEncodings.Peek() == HttpEncoding.CHUNKED) {
                // oh no the poop is chunky
                transferEncodings.Pop();
                body = new MemoryStream();

                while((line = readLine()) != null) {
                    if(string.IsNullOrWhiteSpace(line))
                        break;
                    if(!int.TryParse(line, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int chunkLength))
                        throw new IOException(@"Failed to decode chunk length.");
                    if(chunkLength == 0) // final chunk
                        break;
                    readBuffer(chunkLength);
                    readLine();
                }
            } else if(contentLength != 0) {
                body = new MemoryStream();
                readBuffer();
            }

            if(body != null)
                // Check if body is empty and null it again if so
                if(body.Length == 0) {
                    body.Dispose();
                    body = null;
                } else {
                    if(transferEncodings != null)
                        ProcessEncoding(transferEncodings, body, true);
                    if(contentEncodings != null)
                        ProcessEncoding(contentEncodings, body, false);

                    body.Seek(0, SeekOrigin.Begin);
                }

            return new HttpResponseMessage(statusCode, statusMessage, protocolVersion, headers, body);
        }
    }
}