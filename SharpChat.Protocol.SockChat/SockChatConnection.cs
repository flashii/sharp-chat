using Fleck;
using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Sessions;
using System;
using System.Net;

namespace SharpChat.Protocol.SockChat {
    public class SockChatConnection : IConnection {
        public const int ID_LENGTH = 16;

        public string ConnectionId { get; }
        public IPAddress RemoteAddress { get; }
        public bool IsSecure { get; }

        public bool IsAvailable => Connection.IsAvailable;

        public ClientCapability Capabilities { get; set; }

        private IWebSocketConnection Connection { get; }
        private readonly object Sync = new();

        public IChannel LastChannel { get; set; }

        public DateTimeOffset LastPing { get; set; }
        public ISession Session { get; set; }

        public SockChatConnection(IWebSocketConnection conn) {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            ConnectionId = @"SC!" + RNG.NextString(ID_LENGTH);
            IPAddress remoteAddr = IPAddress.Parse(Connection.ConnectionInfo.ClientIpAddress);
            RemoteAddress = IPAddress.IsLoopback(remoteAddr)
                && Connection.ConnectionInfo.Headers.ContainsKey(@"X-Real-IP")
                ? IPAddress.Parse(Connection.ConnectionInfo.Headers[@"X-Real-IP"])
                : remoteAddr;
        }

        public bool HasCapability(ClientCapability capability)
            => (Capabilities & capability) == capability;

        public void SendPacket(IServerPacket packet) {
            lock(Sync) {
                if(!Connection.IsAvailable)
                    return;
                Connection.Send(packet.Pack());
            }
        }

        public void Close() {
            lock(Sync)
                Connection.Close();
        }

        public override string ToString()
            => $@"C#{ConnectionId}";
    }
}
