using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Hamakaze {
    public class HttpConnectionManager : IDisposable {
        private List<HttpConnection> Connections { get; } = new();
        private Mutex Lock { get; } = new();

        public HttpConnectionManager() {
        }

        private void AcquireLock() {
            if(!Lock.WaitOne(10000))
                throw new HttpConnectionManagerLockException();
        }

        private void ReleaseLock() {
            Lock.ReleaseMutex();
        }

        public HttpConnection CreateConnection(string host, IPEndPoint endPoint, bool secure) {
            if(host == null)
                throw new ArgumentNullException(nameof(host));
            if(endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));
            HttpConnection conn = null;
            AcquireLock();
            try {
                conn = CreateConnectionInternal(host, endPoint, secure);
            } finally {
                ReleaseLock();
            }
            return conn;
        }

        private HttpConnection CreateConnectionInternal(string host, IPEndPoint endPoint, bool secure) {
            HttpConnection conn = new(host, endPoint, secure);
            Connections.Add(conn);
            return conn;
        }

        public HttpConnection GetConnection(string host, IPEndPoint endPoint, bool secure) {
            if(host == null)
                throw new ArgumentNullException(nameof(host));
            if(endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));
            HttpConnection conn = null;
            AcquireLock();
            try {
                conn = GetConnectionInternal(host, endPoint, secure);
            } finally {
                ReleaseLock();
            }
            return conn;
        }

        private HttpConnection GetConnectionInternal(string host, IPEndPoint endPoint, bool secure) {
            CleanConnectionsInternal();
            HttpConnection conn = Connections.FirstOrDefault(c => host.Equals(c.Host) && endPoint.Equals(c.EndPoint) && c.IsSecure == secure && c.Acquire());
            if(conn == null) {
                conn = CreateConnectionInternal(host, endPoint, secure);
                conn.Acquire();
            }
            return conn;
        }

        public void EndConnection(HttpConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            AcquireLock();
            try {
                EndConnectionInternal(conn);
            } finally {
                ReleaseLock();
            }
        }

        private void EndConnectionInternal(HttpConnection conn) {
            Connections.Remove(conn);
            conn.Dispose();
        }

        public void CleanConnection() {
            AcquireLock();
            try {
                CleanConnectionsInternal();
            } finally {
                ReleaseLock();
            }
        }

        private void CleanConnectionsInternal() {
            IEnumerable<HttpConnection> conns = Connections.Where(x => x.HasTimedOut).ToArray();
            foreach(HttpConnection conn in conns) {
                Connections.Remove(conn);
                conn.Dispose();
            }
        }

        private bool IsDisposed;
        ~HttpConnectionManager()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            Lock.Dispose();

            foreach(HttpConnection conn in Connections)
                conn.Dispose();
            Connections.Clear();
        }
    }
}
