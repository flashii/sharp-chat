using System;
using System.Net;

namespace SharpChat.Protocol {
    public interface IServer : IDisposable {
        void Listen(EndPoint endPoint);
    }
}
