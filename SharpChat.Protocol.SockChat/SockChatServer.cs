using SharpChat.Events;
using SharpChat.Protocol.SockChat.Commands;
using SharpChat.Protocol.SockChat.PacketHandlers;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.RateLimiting;
using SharpChat.Sessions;
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

        private HashSet<SockChatConnection> Connections { get; } = new HashSet<SockChatConnection>();
        private IReadOnlyDictionary<ClientPacketId, IPacketHandler> PacketHandlers { get; }

        private readonly object Sync = new object();

        public SockChatServer(Context ctx) {
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
            Context.AddEventHandler(this);

            Dictionary<ClientPacketId, IPacketHandler> handlers = new Dictionary<ClientPacketId, IPacketHandler>();
            void addHandler(IPacketHandler handler) {
                handlers.Add(handler.PacketId, handler);
            };

            addHandler(new PingPacketHandler(Context.Sessions));
            addHandler(new AuthPacketHandler(Context.Sessions, Context.Users, Context.Channels, Context.ChannelUsers, Context.Messages, Context.DataProvider, Context.Bot));
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
            lock(Sync)
                Connections.Add(conn);
        }

        private void OnClose(SockChatConnection conn) {
            Logger.Debug($@"[{conn}] Connection closed");
            lock(Sync) {
                Context.Sessions.Destroy(conn);
                Context.RateLimiter.ClearConnection(conn);
                Connections.Remove(conn);
            }
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
            lock(Sync)
                switch(evt) {
                    case SessionPingEvent spe:
                        SockChatConnection spec = Connections.FirstOrDefault(c => c.Session != null && spe.SessionId.Equals(c.Session.SessionId));
                        if(spec == null)
                            break;
                        spec.LastPing = spe.DateTime;
                        spec.SendPacket(new PongPacket(spe));
                        break;
                    case SessionChannelSwitchEvent scwe:
                        SockChatConnection scwec = Connections.FirstOrDefault(c => c.Session != null && scwe.SessionId.Equals(c.Session.SessionId));
                        if(scwec == null)
                            break;
                        if(scwe.Channel != null)
                            scwec.LastChannel = scwe.Channel;
                        scwec.SendPacket(new ChannelSwitchPacket(scwec.LastChannel));
                        break;
                    case SessionDestroyEvent sde:
                        SockChatConnection sdec = Connections.FirstOrDefault(c => c.Session != null && sde.SessionId.Equals(c.Session.SessionId));
                        if(sdec == null)
                            break;
                        sdec.Close();
                        break;

                    case UserUpdateEvent uue:
                        UserUpdatePacket uuep = new UserUpdatePacket(uue);
                        Context.ChannelUsers.GetUsers(uue.User, users => {
                            foreach(IUser user in users) {
                                SockChatConnection uuec = Connections.FirstOrDefault(c => c.Session != null && user.Equals(c.Session.User));
                                if(uuec == null)
                                    break;
                                uuec.SendPacket(uuep);
                            }
                        });
                        break;
                    case UserDisconnectEvent ude:
                        UserDisconnectPacket udep = new UserDisconnectPacket(ude);
                        Context.ChannelUsers.GetUsers(ude.User, users => {
                            foreach(IUser user in users) {
                                SockChatConnection udec = Connections.FirstOrDefault(c => c.Session != null && user.Equals(c.Session.User));
                                if(udec == null)
                                    break;
                                udec.SendPacket(udep);
                            }
                        });
                        break;

                    case ChannelUserJoinEvent cje: // should send UserConnectPacket on first channel join
                        ChannelJoinPacket cjep = new ChannelJoinPacket(cje);
                        Context.ChannelUsers.GetUsers(cje.Channel, users => {
                            foreach(IUser user in users) {
                                SockChatConnection cjec = Connections.FirstOrDefault(c => c.Session != null && user.Equals(c.Session.User));
                                if(cjec == null)
                                    break;
                                cjec.SendPacket(cjep);
                            }
                        });
                        break;
                    case ChannelUserLeaveEvent cle:
                        ChannelLeavePacket clep = new ChannelLeavePacket(cle);
                        Context.ChannelUsers.GetUsers(cle.Channel, users => {
                            foreach(IUser user in users) {
                                SockChatConnection clec = Connections.FirstOrDefault(c => c.Session != null && user.Equals(c.Session.User));
                                if(clec == null)
                                    break;
                                clec.SendPacket(clep);
                            }
                        });
                        break;

                    case MessageCreateEvent mce:
                        MessageCreatePacket mcep = new MessageCreatePacket(mce);
                        Context.ChannelUsers.GetUsers(mce.Channel, users => {
                            foreach(IUser user in users) {
                                SockChatConnection mcec = Connections.FirstOrDefault(c => c.Session != null && user.Equals(c.Session.User));
                                if(mcec == null)
                                    break;
                                mcec.SendPacket(mcep);
                            }
                        });
                        break;
                    case MessageDeleteEvent mde:
                        MessageDeletePacket mdep = new MessageDeletePacket(mde);
                        Context.ChannelUsers.GetUsers(mde.Channel, users => {
                            foreach(IUser user in users) {
                                SockChatConnection mdec = Connections.FirstOrDefault(c => c.Session != null && user.Equals(c.Session.User));
                                if(mdec == null)
                                    break;
                                mdec.SendPacket(mdep);
                            }
                        });
                        break;
                    case MessageUpdateEvent mue:
                        Context.Messages.GetMessage(mue.MessageId, msg => {
                            if(msg == null)
                                return;

                            MessageDeletePacket muepd = new MessageDeletePacket(mue);
                            MessageCreatePacket muecd = new MessageCreatePacket(new MessageCreateEvent(msg));

                            Context.ChannelUsers.GetUsers(mue.Channel, users => {
                                foreach(IUser user in users) {
                                    SockChatConnection muec = Connections.FirstOrDefault(c => c.Session != null && user.Equals(c.Session.User));
                                    if(muec == null)
                                        break;
                                    muec.SendPacket(muepd);
                                    muec.SendPacket(muecd);
                                }
                            });
                        });
                        break;

                    case BroadcastMessageEvent bme:
                        BroadcastMessagePacket bmep = new BroadcastMessagePacket(bme);
                        foreach(SockChatConnection bmec in Connections) {
                            if(bmec.Session == null)
                                continue;
                            bmec.SendPacket(bmep);
                        }
                        break;
                }
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
