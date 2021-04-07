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
    public class SockChatServer : IServer {
        public const int DEFAULT_MAX_CONNECTIONS = 5;

        private Context Context { get; }
        private FleckWebSocketServer Server { get; set; }

        private List<SockChatConnection> Connections { get; } = new List<SockChatConnection>();
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
            ISession sess = Context.Sessions.GetLocalSession(conn);
            bool hasSession = sess != null;

            RateLimitState rateLimit = RateLimitState.None;
            if(!hasSession || !Context.RateLimiter.HasRankException(sess.User))
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
                    Context.BanUser(sess.User, Context.RateLimiter.BanDuration, UserDisconnectReason.Flood);
                    return;
                } /*else if(rateLimit == RateLimitState.Warn)
                    sess.SendPacket(new FloodWarningPacket(Context.Bot));*/
            }

            if(PacketHandlers.TryGetValue(packetId, out IPacketHandler handler))
                handler.HandlePacket(new PacketHandlerContext(args, sess, conn));
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync)
                switch(evt) {
                    //
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
            Server?.Dispose();
        }
    }
}
