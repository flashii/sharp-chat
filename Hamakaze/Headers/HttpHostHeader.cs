using System;
using System.Linq;
using System.Text;

namespace Hamakaze.Headers {
    public class HttpHostHeader : HttpHeader {
        public const string NAME = @"Host";

        public override string Name => NAME;
        public override object Value {
            get {
                StringBuilder sb = new();
                sb.Append(Host);
                if(Port != -1)
                    sb.AppendFormat(@":{0}", Port);
                return sb.ToString();
            }
        }

        public string Host { get; }
        public int Port { get; }
        public bool IsSecure { get; }

        public HttpHostHeader(string host, int port) {
            Host = host;
            Port = port;
        }

        public HttpHostHeader(string hostAndPort) {
            string[] parts = hostAndPort.Split(':', 2, StringSplitOptions.TrimEntries);
            Host = parts.ElementAtOrDefault(0) ?? throw new ArgumentNullException(nameof(hostAndPort));
            if(!ushort.TryParse(parts.ElementAtOrDefault(1), out ushort port))
                throw new FormatException(@"Host is not in valid format.");
            Port = port;
        }
    }
}
