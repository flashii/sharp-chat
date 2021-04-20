using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Protocol.SockChat.Commands;
using SharpChat.Protocol.SockChat.PacketHandlers;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.RateLimiting;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Protocol.SockChat {
    [Server(@"sockchat")]
    public class SockChatServer : IServer {
        public const int DEFAULT_MAX_CONNECTIONS = 5;

        private Context Context { get; }
        private FleckWebSocketServer Server { get; set; }

        private ConnectionList<SockChatConnection> Connections { get; }
        private IReadOnlyDictionary<ClientPacketId, IPacketHandler> PacketHandlers { get; }

        private CachedValue<string> WelcomeMessageValue { get; }

        public string WelcomeMessage => WelcomeMessageValue;

        public SockChatServer(Context ctx, IConfig config) {
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));

            if(config == null)
                throw new ArgumentNullException(nameof(config));

            WelcomeMessageValue = config.ReadCached(@"welcome", string.Empty);

            Context.AddEventHandler(this);

            Connections = new ConnectionList<SockChatConnection>(Context.ChannelUsers);

            Dictionary<ClientPacketId, IPacketHandler> handlers = new Dictionary<ClientPacketId, IPacketHandler>();
            void addHandler(IPacketHandler handler) {
                handlers.Add(handler.PacketId, handler);
            };

            addHandler(new PingPacketHandler(Context.Sessions));
            addHandler(new AuthPacketHandler(
                this,
                Context.Sessions,
                Context.Users,
                Context.Channels,
                Context.ChannelUsers,
                Context.Messages,
                Context.DataProvider,
                Context.Bot,
                Context.WelcomeMessage
            ));
            addHandler(new MessageSendPacketHandler(Context.Users, Context.Channels, Context.ChannelUsers, Context.Messages, Context.Bot, new ICommand[] {
                new JoinCommand(Context.Channels, Context.ChannelUsers, Context.Sessions),
                new AFKCommand(Context.Users),
                new WhisperCommand(),
                new ActionCommand(Context.Messages),
                new WhoCommand(Context.Users, Context.Channels, Context.ChannelUsers, Context.Bot),
                new DeleteMessageCommand(Context.Messages),

                new NickCommand(Context.Users),
                new CreateChannelCommand(Context.Channels, Context.ChannelUsers, Context.Bot),
                new DeleteChannelCommand(Context.Channels, Context.Bot),
                new ChannelPasswordCommand(Context.Channels, Context.Bot),
                new ChannelRankCommand(Context.Channels, Context.Bot),

                new BroadcastCommand(Context),
                new KickBanUserCommand(Context.Users),
                new PardonUserCommand(Context.DataProvider, Context.Bot),
                new PardonIPCommand(Context.DataProvider, Context.Bot),
                new BanListCommand(Context.DataProvider, Context.Bot),
                new WhoIsUserCommand(Context.Users, Context.Sessions, Context.Bot),
                new SilenceUserCommand(Context.Users, Context.Bot),
                new UnsilenceUserCommand(Context.Users, Context.Bot),
            }));
            addHandler(new CapabilitiesPacketHandler(Context.Sessions));
            addHandler(new TypingPacketHandler());

            PacketHandlers = handlers;

        }

        public void Listen(EndPoint endPoint) {
            if(Server != null)
                throw new ProtocolAlreadyListeningException();
            if(endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));
            if(endPoint is not IPEndPoint ipEndPoint)
                throw new ArgumentException(@"EndPoint must be an IPEndPoint", nameof(endPoint));

            Server = new FleckWebSocketServer(ipEndPoint, false);
            Server.Start(rawConn => {
                SockChatConnection conn = new SockChatConnection(rawConn);
                rawConn.OnOpen += () => OnOpen(conn);
                rawConn.OnClose += () => OnClose(conn);
                rawConn.OnError += ex => OnError(conn, ex);
                rawConn.OnMessage += msg => OnMessage(conn, msg);
            });
        }

        private void OnOpen(SockChatConnection conn) {
            Logger.Debug($@"[{conn}] Connection opened");
            Connections.AddConnection(conn);
        }

        private void OnClose(SockChatConnection conn) {
            Logger.Debug($@"[{conn}] Connection closed");
            Connections.RemoveConnection(conn);
            Context.Sessions.Destroy(conn);
            Context.RateLimiter.ClearConnection(conn);
        }

        private static void OnError(SockChatConnection conn, Exception ex) {
            Logger.Write($@"[{conn}] {ex}");
        }

        private void OnMessage(SockChatConnection conn, string msg) {
            bool hasSession = conn.Session != null;

            RateLimitState rateLimit = RateLimitState.None;
            if(!hasSession || !Context.RateLimiter.HasRankException(conn.Session.User))
                rateLimit = Context.RateLimiter.BumpConnection(conn);
            
            Logger.Debug($@"[{conn}] {rateLimit}");
            if(!hasSession && rateLimit == RateLimitState.Drop) {
                conn.Close();
                return;
            }

            IEnumerable<string> args = msg.Split(IServerPacket.SEPARATOR);
            if(!Enum.TryParse(args.ElementAtOrDefault(0), out ClientPacketId packetId))
                return;

            if(packetId != ClientPacketId.Authenticate) {
                if(!hasSession) 
                    return;

                if(rateLimit == RateLimitState.Drop) {
                    Context.BanUser(conn.Session.User, Context.RateLimiter.BanDuration, UserDisconnectReason.Flood);
                    return;
                } /*else if(rateLimit == RateLimitState.Warn)
                    sess.SendPacket(new FloodWarningPacket(Context.Bot));*/
            }

            if(PacketHandlers.TryGetValue(packetId, out IPacketHandler handler))
                handler.HandlePacket(new PacketHandlerContext(args, conn));
        }

        // the implementation of Everything here needs to be revised
        // probably needs to be something that can more directly associate connections with user( id)s and session( id)s
        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case SessionPingEvent spe:
                    Connections.GetConnectionBySessionId(spe.SessionId, conn => {
                        if(conn == null)
                            return;
                        conn.LastPing = spe.DateTime;
                        conn.SendPacket(new PongPacket(spe));
                    });
                    break;
                case SessionChannelSwitchEvent scwe:
                    Connections.GetConnectionBySessionId(scwe.SessionId, conn => {
                        if(conn == null)
                            return;
                        if(string.IsNullOrEmpty(scwe.ChannelId))
                            Context.Channels.GetChannelById(scwe.ChannelId, channel => {
                                if(channel != null)
                                    conn.LastChannel = channel;
                                conn.SendPacket(new ChannelSwitchPacket(conn.LastChannel));
                            });
                    });
                    break;
                case SessionDestroyEvent sde:
                    Connections.GetConnectionBySessionId(sde.SessionId, conn => {
                        if(conn == null)
                            return;
                        conn.Close();
                    });
                    break;
                case SessionResumeEvent sre:
                    if(string.IsNullOrWhiteSpace(sre.ConnectionId))
                        break;
                    Connections.GetConnection(sre.ConnectionId, conn => {
                        if(conn == null)
                            return;
                        Context.Sessions.GetSession(sre.SessionId, sess => {
                            if(sess == null)
                                return;
                            sess.Connection = conn;
                            conn.Session = sess;
                        });
                    });
                    break;

                case UserUpdateEvent uue:
                    UserUpdatePacket uuep = new UserUpdatePacket(uue);
                    Connections.GetAllConnectionsByUserId(uue.UserId, conns => {
                        foreach(SockChatConnection conn in conns)
                            conn.SendPacket(uuep);
                    });
                    break;
                case UserDisconnectEvent ude:
                    Context.Users.GetUser(ude.UserId, user => {
                        UserDisconnectPacket udep = new UserDisconnectPacket(ude, user);
                        Connections.GetAllConnectionsByUserId(ude.UserId, conns => {
                            foreach(SockChatConnection conn in conns)
                                conn.SendPacket(udep);
                        });
                    });
                    break;

                case ChannelSessionJoinEvent csje:
                    UserJoinChannel(csje.ChannelId, csje.SessionId);
                    break;

                case ChannelUserJoinEvent cuje: // should send UserConnectPacket on first channel join
                    Context.Users.GetUser(cuje.UserId, user => {
                        ChannelJoinPacket cjep = new ChannelJoinPacket(cuje, user);
                        Connections.GetConnectionsByChannelId(cuje.ChannelId, conns => {
                            conns = conns.Where(c => !user.Equals(c.Session?.User));
                            foreach(SockChatConnection conn in conns)
                                conn.SendPacket(cjep);
                        });
                    });

                    UserJoinChannel(cuje.ChannelId, cuje.SessionId);
                    break;
                case ChannelUserLeaveEvent cle:
                    ChannelLeavePacket clep = new ChannelLeavePacket(cle);
                    Connections.GetConnectionsByChannelId(cle.ChannelId, conns => {
                        foreach(SockChatConnection conn in conns)
                            conn.SendPacket(clep);
                    });
                    break;

                case MessageCreateEvent mce:
                    Context.Channels.GetChannelById(mce.ChannelId, channel => {
                        if(channel == null)
                            return;
                        MessageCreatePacket mcep = new MessageCreatePacket(mce.MessageId, mce.UserId, mce.DateTime, channel, mce.Text, mce.IsAction);
                        Connections.GetConnectionsByChannelId(mce.ChannelId, conns => {
                            foreach(SockChatConnection conn in conns)
                                conn.SendPacket(mcep);
                        });
                    });
                    break;
                case MessageDeleteEvent mde:
                    MessageDeletePacket mdep = new MessageDeletePacket(mde);
                    Connections.GetConnectionsByChannelId(mde.ChannelId, conns => {
                        foreach(SockChatConnection conn in conns)
                            conn.SendPacket(mdep);
                    });
                    break;
                case MessageUpdateEvent mue:
                    Context.Messages.GetMessage(mue.MessageId, msg => {
                        if(msg == null)
                            return;

                        Context.Channels.GetChannel(msg.Channel, channel => {
                            if(channel == null)
                                return;

                            MessageDeletePacket muepd = new MessageDeletePacket(mue);
                            MessageCreatePacket muecd = new MessageCreatePacket(mue.MessageId, msg.Sender.UserId, mue.DateTime, channel, mue.Text, msg.IsAction);

                            Connections.GetConnectionsByChannelId(mue.ChannelId, conns => {
                                foreach(SockChatConnection conn in conns) {
                                    conn.SendPacket(muepd);
                                    conn.SendPacket(muecd);
                                }
                            });
                        });
                    });
                    break;

                case BroadcastMessageEvent bme:
                    BroadcastMessagePacket bmep = new BroadcastMessagePacket(bme);
                    Connections.GetConnectionsWithSession(conns => {
                        foreach(SockChatConnection conn in conns)
                            conn.SendPacket(bmep);
                    });
                    break;
            }
        }

        private void UserJoinChannel(string channelId, string sessionId) {
            Context.Sessions.GetLocalSession(sessionId, session => {
                if(session == null || session.Connection is not SockChatConnection conn)
                    return;

                Context.Channels.GetChannelById(channelId, channel => {
                    Context.ChannelUsers.GetUsers(channel, users => conn.SendPacket(
                        new ContextUsersPacket(users.Except(new[] { session.User }).OrderByDescending(u => u.Rank))
                    ));

                    Context.Messages.GetMessages(channel, msgs => {
                        foreach(IMessage msg in msgs)
                            conn.SendPacket(new ContextMessagePacket(msg));
                    });
                });
            });
        }

        private bool IsDisposed;
        ~SockChatServer()
            => DoDispose();
        public void Dispose() { 
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Context.RemoveEventHandler(this);
            Server?.Dispose();
        }
    }
}
