﻿using SharpChat.Events;
using SharpChat.Protocol;
using SharpChat.Users;
using System;
using System.Net;

namespace SharpChat.Sessions {
    public class Session : ISession {
        public const int ID_LENGTH = 32;

        public string SessionId { get; }
        public string ServerId { get; private set; }
        public bool IsSecure { get; }

        public DateTimeOffset LastPing { get; private set; }
        public IUser User { get; }

        public bool IsConnected { get; private set; }
        public IPAddress RemoteAddress { get; private set; }

        private readonly object Sync = new();

        public IConnection Connection { get; set; }

        private long LastEvent { get; set; } // use this to get a session back up to speed after reconnection

        public Session(
            string serverId,
            string sessionId,
            bool isSecure,
            DateTimeOffset? lastPing,
            IUser user,
            bool isConnected,
            IConnection connection,
            IPAddress remoteAddress
        ) {
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            IsSecure = isSecure;
            LastPing = lastPing ?? DateTimeOffset.MinValue;
            User = user;
            IsConnected = isConnected;
            Connection = connection;
            RemoteAddress = remoteAddress ?? IPAddress.None;
        }

        public bool HasConnection(IConnection conn)
            => Connection == conn;

        public void BumpPing()
            => LastPing = DateTimeOffset.Now;

        public bool Equals(ISession other)
            => other != null && SessionId.Equals(other.SessionId);

        public override string ToString()
            => $@"S#{ServerId}#{SessionId}";

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync) {
                switch(evt) {
                    case SessionPingEvent spe:
                        LastPing = spe.DateTime;
                        break;
                    case SessionSuspendEvent _:
                        IsConnected = false;
                        Connection = null;
                        RemoteAddress = IPAddress.None;
                        ServerId = string.Empty;
                        LastPing = DateTimeOffset.Now;
                        break;
                    case SessionResumeEvent sre:
                        IsConnected = true;
                        RemoteAddress = sre.RemoteAddress;
                        ServerId = sre.ServerId;
                        LastPing = DateTimeOffset.Now; // yes?
                        break;
                    case SessionDestroyEvent _:
                        IsConnected = false;
                        LastPing = DateTimeOffset.MinValue;
                        break;
                    /*case SessionResumeEvent _:
                        while(PacketQueue.TryDequeue(out IServerPacket packet))
                            SendPacket(packet);
                        PacketQueue = null;
                        break;*/
                }

                if(Connection != null)
                    LastEvent = evt.EventId;
            }
        }
    }
}
