using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;

namespace Hamakaze {
    public class HttpConnection : IDisposable {
        public IPEndPoint EndPoint { get; }
        public Stream Stream { get; }

        public Socket Socket { get; }
        public NetworkStream NetworkStream { get; }
        public SslStream SslStream { get; }

        public string Host { get; }
        public bool IsSecure { get; }

        public bool HasTimedOut => MaxRequests == 0 || (DateTimeOffset.Now - LastOperation) > MaxIdle;

        public int MaxRequests { get; set; } = -1;
        public TimeSpan MaxIdle { get; set; } = TimeSpan.MaxValue;
        public DateTimeOffset LastOperation { get; private set; } = DateTimeOffset.Now;

        public bool InUse { get; private set; }

        public HttpConnection(string host, IPEndPoint endPoint, bool secure) {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            IsSecure = secure;

            if(endPoint.AddressFamily is not AddressFamily.InterNetwork and not AddressFamily.InterNetworkV6)
                throw new ArgumentException(@"Address must be an IPv4 or IPv6 address.", nameof(endPoint));

            Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {
                NoDelay = true,
                Blocking = true,
            };
            Socket.Connect(endPoint);

            NetworkStream = new NetworkStream(Socket, true);

            if(IsSecure) {
                SslStream = new SslStream(NetworkStream, false, (s, ce, ch, e) => e == SslPolicyErrors.None, null);
                Stream = SslStream;
                SslStream.AuthenticateAsClient(Host, null, SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13, true);
            } else
                Stream = NetworkStream;
        }

        public void MarkUsed() {
            LastOperation = DateTimeOffset.Now;
            if(MaxRequests > 0)
                --MaxRequests;
        }

        public bool Acquire() {
            return !InUse && (InUse = true);
        }

        public void Release() {
            InUse = false;
        }

        private bool IsDisposed;
        ~HttpConnection()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Stream.Dispose();
        }
    }
}
