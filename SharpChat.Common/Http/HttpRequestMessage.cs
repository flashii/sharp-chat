﻿using SharpChat.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpChat.Http {
    public class HttpRequestMessage : HttpMessage {
        public const string GET = @"GET";
        public const string PUT = @"PUT";
        public const string HEAD = @"HEAD";
        public const string POST = @"POST";
        public const string DELETE = @"DELETE";

        public override string ProtocolVersion => @"1.1";

        public string Method { get; }
        public string RequestTarget { get; }

        public bool IsSecure { get; }

        public string Host { get; }
        public ushort Port { get; }
        public bool IsDefaultPort { get; }

        public override IEnumerable<HttpHeader> Headers => HeaderList;
        private List<HttpHeader> HeaderList { get; } = new List<HttpHeader>();

        private bool OwnsBodyStream { get; set; }
        private Stream BodyStream { get; set; }
        public override Stream Body {
            get {
                if(BodyStream == null) {
                    OwnsBodyStream = true;
                    SetBody(new MemoryStream());
                }
                return BodyStream;
            }
        }

        private static readonly string[] HEADERS_READONLY = new[] {
            HttpHostHeader.NAME, HttpContentLengthHeader.NAME,
        };
        private static readonly string[] HEADERS_SINGLE = new[] {
            HttpUserAgentHeader.NAME, HttpConnectionHeader.NAME, HttpAcceptEncodingHeader.NAME,
        };

        public IEnumerable<HttpEncoding> AcceptedEncodings {
            get => HeaderList.Where(x => x.Name == HttpAcceptEncodingHeader.NAME).Cast<HttpAcceptEncodingHeader>().FirstOrDefault()?.Encodings
                ?? Enumerable.Empty<HttpEncoding>();

            set {
                HeaderList.RemoveAll(x => x.Name == HttpAcceptEncodingHeader.NAME);
                HeaderList.Add(new HttpAcceptEncodingHeader(value));
            }
        }

        public string UserAgent {
            get => HeaderList.FirstOrDefault(x => x.Name == HttpUserAgentHeader.NAME)?.Value.ToString()
                ?? string.Empty;
            set {
                HeaderList.RemoveAll(x => x.Name == HttpUserAgentHeader.NAME);
                HeaderList.Add(new HttpUserAgentHeader(value));
            }
        }

        public string Connection {
            get => HeaderList.FirstOrDefault(x => x.Name == HttpConnectionHeader.NAME)?.Value.ToString()
                ?? string.Empty;
            set {
                HeaderList.RemoveAll(x => x.Name == HttpConnectionHeader.NAME);
                HeaderList.Add(new HttpConnectionHeader(value));
            }
        }

        public HttpMediaType ContentType {
            get => HeaderList.Where(x => x.Name == HttpContentTypeHeader.NAME).Cast<HttpContentTypeHeader>().FirstOrDefault()?.MediaType
                ?? HttpMediaType.OctetStream;
            set {
                HeaderList.RemoveAll(x => x.Name == HttpContentTypeHeader.NAME);
                HeaderList.Add(new HttpContentTypeHeader(value));
            }
        }

        public HttpRequestMessage(string method, string uri) : this(
            method, new Uri(uri)
        ) {}

        public const ushort HTTP = 80;
        public const ushort HTTPS = 443;

        public HttpRequestMessage(string method, Uri uri) {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            RequestTarget = uri.PathAndQuery;
            IsSecure = uri.Scheme.Equals(@"https", StringComparison.InvariantCultureIgnoreCase);
            Host = uri.Host;
            ushort defaultPort = (IsSecure ? HTTPS : HTTP);
            Port = uri.Port == -1 ? defaultPort : (ushort)uri.Port;
            IsDefaultPort = Port == defaultPort;
            HeaderList.Add(new HttpHostHeader(Host, IsDefaultPort ? -1 : Port));
        }

        public static bool IsHeaderReadOnly(string name)
            => HEADERS_READONLY.Contains(name ?? throw new ArgumentNullException(nameof(name)));
        public static bool IsHeaderSingleInstance(string name)
            => HEADERS_SINGLE.Contains(name ?? throw new ArgumentNullException(nameof(name)));

        public void SetHeader(string name, object value) {
            name = HttpHeader.NormaliseName(name ?? throw new ArgumentNullException(nameof(name)));
            if(IsHeaderReadOnly(name))
                throw new ArgumentException(@"This header is read-only.", nameof(name));
            HeaderList.RemoveAll(x => x.Name == name);
            HeaderList.Add(HttpHeader.Create(name, value));
        }

        public void AddHeader(string name, object value) {
            name = HttpHeader.NormaliseName(name ?? throw new ArgumentNullException(nameof(name)));
            if(IsHeaderReadOnly(name))
                throw new ArgumentException(@"This header is read-only.", nameof(name));
            if(IsHeaderSingleInstance(name))
                HeaderList.RemoveAll(x => x.Name == name);
            HeaderList.Add(HttpHeader.Create(name, value));
        }

        public void RemoveHeader(string name) {
            name = HttpHeader.NormaliseName(name ?? throw new ArgumentNullException(nameof(name)));
            if(IsHeaderReadOnly(name))
                throw new ArgumentException(@"This header is read-only.", nameof(name));
            HeaderList.RemoveAll(x => x.Name == name);
        }

        public void SetBody(Stream stream) {
            if(stream == null) {
                if(OwnsBodyStream)
                    BodyStream?.Dispose();
                OwnsBodyStream = false;
                BodyStream = null;
                HeaderList.RemoveAll(x => x.Name == HttpContentLengthHeader.NAME);
            } else {
                if(!BodyStream.CanRead || !BodyStream.CanSeek)
                    throw new ArgumentException(@"Body must readable and seekable.", nameof(stream));
                if(OwnsBodyStream)
                    BodyStream?.Dispose();
                OwnsBodyStream = false;
                BodyStream = stream;
                HeaderList.Add(new HttpContentLengthHeader(BodyStream));
            }
        }

        public void WriteTo(Stream stream) {
            using(StreamWriter sw = new StreamWriter(stream, new ASCIIEncoding(), leaveOpen: true)) {
                sw.NewLine = "\r\n";
                sw.Write(Method);
                Console.Write(Method);
                sw.Write(' ');
                Console.Write(' ');
                sw.Write(RequestTarget);
                Console.Write(RequestTarget);
                sw.Write(@" HTTP/");
                Console.Write(@" HTTP/");
                sw.WriteLine(ProtocolVersion);
                Console.WriteLine(ProtocolVersion);
                foreach(HttpHeader header in Headers) {
                    sw.WriteLine(header);
                    Console.WriteLine(header);
                }
                sw.WriteLine();
                Console.WriteLine();
                sw.Flush();
            }
            BodyStream?.CopyTo(stream);
        }
    }
}