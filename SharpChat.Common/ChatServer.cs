﻿using SharpChat.Commands;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.PacketHandlers;
using SharpChat.Packets;
using SharpChat.RateLimiting;
using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class ChatServer : IDisposable {
        private const int VERSION =
#if DEBUG
            2;
#else
            1;
#endif

        public const int DEFAULT_MAX_CONNECTIONS = 5;
        public const int ID_LENGTH = 8;

        public string ServerId { get; }
        private IConfig Config { get; }
        private IServer Server { get; }
        private ChatContext Context { get; }

        private IReadOnlyCollection<IPacketHandler> PacketHandlers { get; }

        public bool AcceptingConnections { get; private set; }

        public ChatServer(IConfig config, IServer server, IDataProvider dataProvider, IDatabaseBackend databaseBackend) {
            Logger.Write("Starting Sock Chat server...");

            ServerId = RNG.NextString(ID_LENGTH); // maybe read this from the cfg instead
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Context = new ChatContext(ServerId, Config.ScopeTo(@"chat"), databaseBackend, dataProvider);

            List<IPacketHandler> handlers = new List<IPacketHandler> {
                new PingPacketHandler(Context.Sessions),
                new AuthPacketHandler(Context.Sessions, Context.Users, Context.Channels, Context.ChannelUsers, Context.Messages, Context.DataProvider, Context.Bot, VERSION),
                new MessageSendPacketHandler(Context.Users, Context.Channels, Context.ChannelUsers, Context.Messages, Context.Bot, new ICommand[] {
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
                }),
            };

            if(VERSION >= 2)
                handlers.AddRange(new IPacketHandler[] {
                    new CapabilitiesPacketHandler(Context.Sessions),
                    new TypingPacketHandler(),
                });

            PacketHandlers = handlers.ToArray();

            Server = server ?? throw new ArgumentNullException(nameof(server));
            Server.OnOpen += OnOpen;
            Server.OnClose += OnClose;
            Server.OnError += OnError;
            Server.OnMessage += OnMessage;
            Server.Start();

            AcceptingConnections = true;
        }

        private void OnOpen(IConnection conn) {
            Logger.Debug($@"[{conn}] Connection opened");

            if(!AcceptingConnections) {
                conn.Dispose();
                return;
            }

            Context.Update();
        }

        private void OnClose(IConnection conn) {
            Logger.Debug($@"[{conn}] Connection closed");
            Context.RateLimiter.ClearConnection(conn);
            Context.Update();
        }

        private void OnError(IConnection conn, Exception ex) {
            ILocalSession sess = Context.Sessions.GetLocalSession(conn);
            Logger.Write($@"[{sess} {conn}] {ex}");
            Context.Update();
        }

        private void OnMessage(IConnection conn, string msg) {
            Context.Update();

            ILocalSession sess = Context.Sessions.GetLocalSession(conn);
            bool hasUser = sess?.HasUser() == true;

            RateLimitState rateLimit = RateLimitState.None;
            if(!hasUser || !Context.RateLimiter.HasRankException(sess.User))
                rateLimit = Context.RateLimiter.BumpConnection(conn);
            
            Logger.Debug($@"[{conn}] {rateLimit}");
            if(!hasUser && rateLimit == RateLimitState.Drop) {
                conn.Dispose();
                return;
            }

            IEnumerable<string> args = msg.Split(IServerPacket.SEPARATOR);
            if(!Enum.TryParse(args.ElementAtOrDefault(0), out ClientPacketId opCode))
                return;

            if(opCode != ClientPacketId.Authenticate) {
                if(!hasUser) 
                    return;

                if(rateLimit == RateLimitState.Drop) {
                    Context.BanUser(sess.User, Context.RateLimiter.BanDuration, UserDisconnectReason.Flood);
                    return;
                } else if(rateLimit == RateLimitState.Warn)
                    sess.SendPacket(new FloodWarningPacket(Context.Bot));
            }

            PacketHandlers.FirstOrDefault(x => x.PacketId == opCode)?.HandlePacket(
                new PacketHandlerContext(args, sess, conn)
            );
        }

        private bool IsDisposed;
        ~ChatServer()
            => DoDispose();
        public void Dispose() { 
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            AcceptingConnections = false;
            Context.Dispose();
        }
    }
}
