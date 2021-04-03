using Fleck;
using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Protocol.SockChat.Packets;
using System;
using System.Net;

namespace SharpChat.Protocol.SockChat {
    public class SockChatConnection : IConnection {
        public const int ID_LENGTH = 16;

        public string ConnectionId { get; }
        public IPAddress RemoteAddress { get; }

        public bool IsAvailable => Connection.IsAvailable;

        public ClientCapability Capabilities { get; set; }

        private IWebSocketConnection Connection { get; }
        private readonly object Sync = new object();

        private IChannel LastChannel { get; set; }

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

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync) {
                switch(evt) {
                    case SessionPingEvent spe:
                        SendPacket(new PongPacket(spe));
                        break;
                    case SessionChannelSwitchEvent scwe:
                        if(scwe.Channel != null)
                            LastChannel = scwe.Channel;
                        SendPacket(new ChannelSwitchPacket(LastChannel));
                        break;
                    case SessionDestroyEvent _:
                        Connection.Close();
                        break;

                    case UserUpdateEvent uue:
                        SendPacket(new UserUpdatePacket(uue));
                        break;

                    case ChannelUserJoinEvent cje: // should send UserConnectPacket on first channel join
                        SendPacket(new ChannelJoinPacket(cje));
                        break;
                    case ChannelUserLeaveEvent cle:
                        SendPacket(new ChannelLeavePacket(cle));
                        break;

                    case UserDisconnectEvent ude:
                        SendPacket(new UserDisconnectPacket(ude));
                        break;

                    case MessageCreateEvent mce:
                        SendPacket(new MessageCreatePacket(mce));
                        break;
                    case MessageUpdateEventWithData muewd:
                        SendPacket(new MessageDeletePacket(muewd));
                        SendPacket(new MessageCreatePacket(muewd));
                        break;
                    case MessageUpdateEvent _:
                        //SendPacket(new MessageUpdatePacket(mue));
                        break;
                    case MessageDeleteEvent mde:
                        SendPacket(new MessageDeletePacket(mde));
                        break;

                    case BroadcastMessageEvent bme:
                        SendPacket(new BroadcastMessagePacket(bme));
                        break;
                }
            }
        }

        public override string ToString()
            => $@"C#{ConnectionId}";
    }
}
