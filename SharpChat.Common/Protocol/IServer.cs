using SharpChat.Events;
using System;
using System.Net;

namespace SharpChat.Protocol {
    public interface IServer : IEventHandler, IDisposable {
        void Listen(EndPoint endPoint);
    }
}
