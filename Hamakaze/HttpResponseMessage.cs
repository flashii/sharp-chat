using Hamakaze.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Hamakaze {
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
        public Encoding ResponseEncoding
            => Encoding.GetEncoding(ContentType.GetParamValue(@"charset") ?? @"iso8859-1");
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
            OwnsBodyStream = true;
            Body = body;
        }

        public byte[] GetBodyBytes() {
            if(Body == null)
                return null;
            if(Body is MemoryStream msBody)
                return msBody.ToArray();
            using MemoryStream ms = new();
            if(Body.CanSeek)
                Body.Seek(0, SeekOrigin.Begin);
            Body.CopyTo(ms);
            return ms.ToArray();
        }

        public string GetBodyString() {
            byte[] bytes = GetBodyBytes();
            return bytes == null || bytes.Length < 1
                ? string.Empty
                : ResponseEncoding.GetString(bytes);
        }

        // there's probably a less stupid way to do this, be my guest and call me an idiot
        private static void ProcessEncoding(Stack<string> encodings, Stream stream, bool transfer) {
            using MemoryStream temp = new();
            bool inTemp = false;

            while(encodings.TryPop(out string encoding)) {
                Stream target = (inTemp = !inTemp) ? temp : stream,
                    source = inTemp ? stream : temp;

                target.SetLength(0);
                source.Seek(0, SeekOrigin.Begin);

                switch(encoding) {
                    case HttpEncoding.GZIP:
                    case HttpEncoding.XGZIP:
                        using(GZipStream gzs = new(source, CompressionMode.Decompress, true))
                            gzs.CopyTo(target);
                        break;

                    case HttpEncoding.DEFLATE:
                        using(DeflateStream def = new(source, CompressionMode.Decompress, true))
                            def.CopyTo(target);
                        break;

                    case HttpEncoding.BROTLI:
                        if(transfer)
                            goto default;
                        using(BrotliStream br = new(source, CompressionMode.Decompress, true))
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

        public static HttpResponseMessage ReadFrom(Stream stream, Action<long, long> onProgress = null) {
            // ignore this function, it doesn't exist
            string readLine() {
                const ushort crlf = 0x0D0A;
                using MemoryStream ms = new();
                int byt; ushort lastTwo = 0;

                for(; ; ) {
                    byt = stream.ReadByte();
                    if(byt == -1 && ms.Length == 0)
                        return null;

                    ms.WriteByte((byte)byt);

                    lastTwo <<= 8;
                    lastTwo |= (byte)byt;
                    if(lastTwo == crlf) {
                        ms.SetLength(ms.Length - 2);
                        break;
                    }
                }

                return Encoding.ASCII.GetString(ms.ToArray());
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
            List<HttpHeader> headers = new();

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

                if(header is HttpContentLengthHeader hclh)
                    contentLength = (long)hclh.Value;
                else if(header is HttpTransferEncodingHeader hteh)
                    transferEncodings = new Stack<string>(hteh.Encodings);
                else if(header is HttpContentEncodingHeader hceh)
                    contentEncodings = new Stack<string>(hceh.Encodings);

                headers.Add(header);
            }

            if(statusCode is < 200 or 201 or 204 or 205)
                contentLength = 0;

            Stream body = null;
            long totalRead = 0;
            const int buffer_size = 8192;
            byte[] buffer = new byte[buffer_size];
            int read;

            void readBuffer(long length = -1) {
                if(length == 0)
                    return;
                long remaining = length;
                int bufferRead = buffer_size;
                if(bufferRead > length)
                    bufferRead = (int)length;

                if(totalRead < 1)
                    onProgress?.Invoke(0, contentLength);

                while((read = stream.Read(buffer, 0, bufferRead)) > 0) {
                    body.Write(buffer, 0, read);

                    totalRead += read;
                    onProgress?.Invoke(totalRead, contentLength);

                    if(length >= 0) {
                        remaining -= read;
                        if(remaining < 1)
                            break;
                        if(bufferRead > remaining)
                            bufferRead = (int)remaining;
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
                readLine();
            } else if(contentLength != 0) {
                body = new MemoryStream();
                readBuffer(contentLength);
                readLine();
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
