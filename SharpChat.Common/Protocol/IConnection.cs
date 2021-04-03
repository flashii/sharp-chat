using SharpChat.Events;
using System.Net;

namespace SharpChat.Protocol {
    public interface IConnection : IEventHandler {
        string ConnectionId { get; }
        IPAddress RemoteAddress { get; }
        bool IsAvailable { get; }
        void Close();
    }
}
