using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Protocol.IRC.ClientCommands;
using SharpChat.Protocol.IRC.ClientCommands.Modern;
using SharpChat.Protocol.IRC.ClientCommands.RFC1459;
using SharpChat.Protocol.IRC.ClientCommands.RFC2810;
using SharpChat.Protocol.IRC.Replies;
using SharpChat.Protocol.IRC.ServerCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SharpChat.Protocol.IRC {
    [Server(@"irc")]
    public class IRCServer : IServer {
        private const int BUFFER_SIZE = 2048;

        public const char PREFIX = ':';

        private Context Context { get; }
        private IConfig Config { get; }
        private Socket Socket { get; set; }

        private IRCConnectionList Connections { get; }
        private IReadOnlyDictionary<string, IClientCommand> Commands { get; }

        private bool IsRunning { get; set; }

        private byte[] Buffer { get; } = new byte[BUFFER_SIZE];

        private readonly object Sync = new object();

        // I feel like these two could be generalised
        private CachedValue<string> ServerHostValue { get; }
        private CachedValue<string> NetworkNameValue { get; }

        public string ServerHost => ServerHostValue;
        public string NetworkName => NetworkNameValue;

        public IRCServer(Context ctx, IConfig config) {
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
            Config = config ?? throw new ArgumentNullException(nameof(config));

            Connections = new IRCConnectionList(Context.ChannelUsers);

            Context.AddEventHandler(this);

            ServerHostValue = Config.ReadCached(@"host", @"irc.example.com");
            NetworkNameValue = Config.ReadCached(@"network", @"SharpChat");

            Dictionary<string, IClientCommand> handlers = new Dictionary<string, IClientCommand>();
            void addHandler(IClientCommand handler) {
                handlers.Add(handler.CommandName, handler);
            };

            // RFC 1459
            addHandler(new AdminCommand(this));
            addHandler(new AwayCommand(Context.Users));
            addHandler(new InfoCommand());
            addHandler(new InviteCommand(Context.Users, Context.Channels, Context.ChannelUsers));
            addHandler(new IsOnCommand(Context.Users));
            addHandler(new JoinCommand(Context.Channels, Context.ChannelUsers));
            addHandler(new KickCommand());
            addHandler(new KillCommand());
            addHandler(new ListCommand(Context.Channels, Context.ChannelUsers));
            addHandler(new ListUsersCommand());
            addHandler(new MessageOfTheDayCommand());
            addHandler(new ModeCommand());
            addHandler(new NamesCommand());
            addHandler(new NickCommand());
            addHandler(new NoticeCommand());
            addHandler(new PartCommand(Context.Channels, Context.ChannelUsers));
            addHandler(new PassCommand());
            addHandler(new PingCommand(this, Context.Sessions));
            addHandler(new PrivateMessageCommand(Context.Channels, Context.ChannelUsers, Context.Messages));
            addHandler(new QuitCommand());
            addHandler(new StatsCommand());
            addHandler(new SummonCommand());
            addHandler(new TimeCommand());
            addHandler(new TopicCommand());
            addHandler(new UserCommand(this, Context, Context.Users, Context.Sessions, Context.DataProvider));
            addHandler(new UserHostCommand());
            addHandler(new VersionCommand());
            addHandler(new WAllOpsCommand());
            addHandler(new WhoCommand());
            addHandler(new WhoIsCommand());
            addHandler(new WhoWasCommand());

            // RFC 2810
            addHandler(new ServiceListCommand());
            addHandler(new ServiceQueryCommand());

            // Modern
            addHandler(new CapabilitiesCommand());
            addHandler(new SilenceCommand());

            Commands = handlers;
        }

        public void Listen(EndPoint endPoint) {
            if(Socket != null)
                throw new ProtocolAlreadyListeningException();
            if(endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));
            Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(endPoint);
            Socket.NoDelay = true;
            Socket.Blocking = false;
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            Socket.Listen(10);

            IsRunning = true;
            new Thread(Worker).Start();
        }

        private void Worker() {
            while(IsRunning)
                try {
                    if(Socket.Poll(1000000, SelectMode.SelectRead))
                        Connections.AddConnection(new IRCConnection(this, Socket.Accept()));

                    Connections.GetReadyConnections(conns => {
                        foreach(IRCConnection conn in conns) {
                            try {
                                int read = conn.Receive(Buffer);
                                string[] lines = Encoding.UTF8.GetString(Buffer, 0, read).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach(string line in lines)
                                    OnReceive(conn, line);
                            } catch(SocketException ex) {
                                Logger.Write($@"[IRC] Socket Error: {ex}");
                            }
                        }

                        // check pings

                        Connections.GetDeadConnections(conns => {
                            Queue<IRCConnection> dead = new Queue<IRCConnection>(conns);
                            while(dead.TryDequeue(out IRCConnection conn)) {
                                Connections.RemoveConnection(conn);
                                Context.Sessions.Destroy(conn);
                            }
                        });
                    });
                } catch(Exception ex) {
                    Logger.Write($@"[IRC] {ex}");
                }
        }

        private void OnReceive(IRCConnection conn, string line) {
            // do rate limiting

            string command = null;
            List<string> args = new List<string>();

            try {
                int i = 0;

                // read prefix, if there is any.
                // might be unnecessary, might only be server to server which will never happen
                if(line[0] == PREFIX)
                    while(line[++i] != ' ');

                int commandStart = i;
                while((i < (line.Length - 1)) && line[++i] != ' ');
                if(line.Length - 1 == i)
                    ++i;
                command = line[commandStart..i];

                int paramStart = ++i;
                while(i < line.Length) {
                    if(line[i] == ' ' && i != paramStart) {
                        args.Add(line[paramStart..i]);
                        paramStart = i + 1;
                    }

                    if(line[i] == PREFIX) {
                        if(paramStart != i)
                            args.Add(line[paramStart..i]);
                        args.Add(line[(i + 1)..]);
                        break;
                    }

                    ++i;
                }

                if(i == line.Length)
                    args.Add(line[paramStart..]);
            } catch(IndexOutOfRangeException) {
                Logger.Debug($@"Invalid message: {line}");
            }

            if(command == null)
                return;

            args.RemoveAll(string.IsNullOrWhiteSpace);

            if(Commands.ContainsKey(command))
                Commands[command].HandleCommand(new ClientCommandContext(conn, args));
        }

        // see comment in SockChatServer class
        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case SessionPingEvent spe:
                    Connections.GetConnectionBySessionId(spe.Session.SessionId, conn => {
                        if(conn == null)
                            return;
                        conn.LastPing = spe.DateTime;
                    });
                    break;

                case MessageCreateEvent mce:
                    Queue<ServerPrivateMessageCommand> msgs = new Queue<ServerPrivateMessageCommand>(ServerPrivateMessageCommand.Split(mce));
                    Connections.GetConnections(mce.Channel, conns => {
                        while(msgs.TryDequeue(out ServerPrivateMessageCommand spmc))
                            foreach(IRCConnection conn in conns)
                                conn.SendCommand(spmc);
                    });
                    break;

                //case MessageUpdateEvent mue:
                    //IMessage msg = Context.Messages.GetMessage(mue.MessageId);

                    //break;


                // these events need revising
                case ChannelUserJoinEvent cuje:
                    ServerJoinCommand sjc = new ServerJoinCommand(cuje.Channel, cuje.User);
                    Connections.GetConnections(cuje.Channel, conns => {
                        foreach(IRCConnection conn in conns)
                            conn.SendCommand(sjc);
                    });

                    UserJoinChannel(cuje.Channel, cuje.SessionId);
                    break;
                case ChannelSessionJoinEvent csje:
                    UserJoinChannel(csje.Channel, csje.SessionId);
                    break;

                case ChannelUserLeaveEvent cule:
                    ServerPartCommand spc = new ServerPartCommand(cule.Channel, cule.User, cule.Reason);
                    Connections.GetConnections(cule.Channel, conns => {
                        foreach(IRCConnection conn in conns)
                            conn.SendCommand(spc);
                    });
                    break;
            }
        }

        private void UserJoinChannel(IChannel channel, string sessionId) {
            Context.Sessions.GetLocalSession(sessionId, session => {
                if(session == null || session.Connection is not IRCConnection csjec)
                    return;

                Context.Channels.GetChannel(channel, channel => {
                    csjec.SendCommand(new ServerJoinCommand(channel, session.User));
                    if(string.IsNullOrEmpty(channel.Topic))
                        csjec.SendReply(new NoTopicReply(channel));
                    else
                        csjec.SendReply(new TopicReply(channel));
                    Context.ChannelUsers.GetUsers(channel, users => NamesReply.SendBatch(csjec, channel, users));
                    csjec.SendReply(new EndOfNamesReply(channel));
                });
            });
        }

        private bool IsDisposed;
        ~IRCServer()
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

            IsRunning = false;

            if(Socket != null)
                lock(Sync) {
                    // kill all connections

                    Socket.Dispose();
                }
        }
    }
}
