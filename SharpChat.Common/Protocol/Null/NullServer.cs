using SharpChat.Events;
using System;
using System.Net;

namespace SharpChat.Protocol.Null {
    [Server(@"null")]
    public class NullServer : IServer {
        public void Listen(EndPoint endPoint) { }

        public void HandleEvent(object sender, IEvent evt) { }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
