using SharpChat.Sessions;
using System;
using System.Net;

namespace SharpChat.Protocol {
    public interface IConnection {
        string ConnectionId { get; }
        IPAddress RemoteAddress { get; }
        bool IsAvailable { get; }
        DateTimeOffset LastPing { get; set; }
        ISession Session { get; set; }
        void Close();
    }
}
