﻿using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;

namespace SharpChat.Http {
    public class HttpClientConnection : IDisposable {
        public IPEndPoint EndPoint { get; }
        public Stream Stream { get; }
        private Mutex Lock { get; }

        public Socket Socket { get; }
        public NetworkStream NetworkStream { get; }
        public SslStream SslStream { get; }

        public string Host { get; }
        public bool IsSecure { get; }

        public HttpClientConnection(string host, IPEndPoint endPoint, bool secure) {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            IsSecure = secure;

            if(endPoint.AddressFamily != AddressFamily.InterNetwork
                && endPoint.AddressFamily != AddressFamily.InterNetworkV6)
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

            Lock = new Mutex();
        }

        public bool Acquire() {
            return Lock.WaitOne(0);
        }

        public void Release() {
            Lock.ReleaseMutex();
        }

        private bool IsDisposed;
        ~HttpClientConnection()
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
            Lock.Dispose();
        }
    }
}
