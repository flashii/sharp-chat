using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat.Sessions {
    public class Session : ISession {
        public const int ID_LENGTH = 32;

        public string SessionId { get; }
        public string ServerId { get; private set; }
        public DateTimeOffset LastPing { get; private set; }
        public IUser User { get; }

        public bool IsConnected { get; private set; }
        public IPAddress RemoteAddress { get; private set; }

        public ClientCapability Capabilities { get; private set; }

        private readonly object Sync = new object();

        private Queue<IServerPacket> PacketQueue { get; set; }
        private IConnection Connection { get; set; }
        private IChannel LastChannel { get; set; }

        public Session(string serverId, IConnection connection, IUser user)
            : this(
                  serverId,
                  RNG.NextString(ID_LENGTH),
                  DateTimeOffset.Now,
                  user,
                  connection != null,
                  connection?.RemoteAddress
            ) {
            Connection = connection;
        }

        public Session(
            string serverId,
            string sessionId,
            DateTimeOffset? lastPing = null,
            IUser user = null,
            bool isConnected = false,
            IPAddress remoteAddress = null,
            ClientCapability capabilities = 0
        ) {
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            LastPing = lastPing ?? DateTimeOffset.MinValue;
            User = user;
            IsConnected = isConnected;
            RemoteAddress = remoteAddress ?? IPAddress.None;
            Capabilities = capabilities;
            if(!IsConnected)
                PacketQueue = new Queue<IServerPacket>();
        }

        public bool HasConnection(IConnection conn)
            => Connection == conn;

        public void BumpPing()
            => LastPing = DateTimeOffset.Now;

        public void SendPacket(IServerPacket packet) {
            lock(Sync) {
                if(!IsConnected) {
                    PacketQueue.Enqueue(packet);
                    return;
                }

                if(!Connection.IsAvailable)
                    return;

                Connection.Send(packet.Pack());
            }
        }

        public bool Equals(ISession other)
            => other != null && SessionId.Equals(other.SessionId);

        public override string ToString()
            => $@"S#{ServerId}#{SessionId}";

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync) {
                HandleEventGeneric(evt);
                if(Connection != null)
                    HandleEventActive(evt);
            }
        }

        private void HandleEventGeneric(IEvent evt) {
            switch(evt) {
                case SessionCapabilitiesEvent sce:
                    Capabilities = sce.Capabilities;
                    break;
                case SessionPingEvent spe:
                    LastPing = spe.DateTime;
                    break;
                /*case SessionChannelSwitchEvent scwe:
                    if(scwe.Channel != null)
                        LastChannel = scwe.Channel;
                    break;*/
                case SessionSuspendEvent _:
                    PacketQueue = new Queue<IServerPacket>();
                    IsConnected = false;
                    Connection = null;
                    RemoteAddress = IPAddress.None;
                    ServerId = string.Empty;
                    LastPing = DateTimeOffset.Now;
                    break;
                case SessionResumeEvent sre:
                    IsConnected = true;
                    if(sre.HasConnection)
                        Connection = sre.Connection;
                    else
                        PacketQueue = null;
                    RemoteAddress = sre.RemoteAddress;
                    ServerId = sre.ServerId;
                    LastPing = DateTimeOffset.Now; // yes?
                    break;
                case SessionDestroyEvent _:
                    IsConnected = false;
                    LastPing = DateTimeOffset.MinValue;
                    break;
            }
        }

        private void HandleEventActive(IEvent evt) {
            if(evt.Channel != null)
                LastChannel = evt.Channel;

            switch(evt) {
                case SessionCapabilitiesEvent sce:
                    SendPacket(new CapabilityConfirmationPacket(sce));
                    break;
                case SessionPingEvent spe:
                    SendPacket(new PongPacket(spe));
                    break;
                case SessionChannelSwitchEvent scwe:
                    if(scwe.Channel != null)
                        LastChannel = scwe.Channel;
                    SendPacket(new ChannelSwitchPacket(LastChannel));
                    break;
                case SessionResumeEvent _:
                    while(PacketQueue.TryDequeue(out IServerPacket packet))
                        SendPacket(packet);
                    PacketQueue = null;
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
}
