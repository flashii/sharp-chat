using SharpChat.Protocol.IRC.Commands;
using SharpChat.Protocol.IRC.Commands.Modern;
using SharpChat.Protocol.IRC.Commands.RFC1459;
using SharpChat.Protocol.IRC.Commands.RFC2810;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SharpChat.Protocol.IRC {
    public class IRCServer : IServer {
        private const int BUFFER_SIZE = 2048;

        public const char PREFIX = ':';

        private Context Context { get; }
        private Socket Socket { get; set; }

        private Dictionary<Socket, IRCConnection> Connections { get; } = new Dictionary<Socket, IRCConnection>();
        private IReadOnlyDictionary<string, ICommand> Commands { get; }

        private bool IsRunning { get; set; }

        private byte[] Buffer { get; } = new byte[BUFFER_SIZE];

        public IRCServer(Context ctx) {
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));

            Dictionary<string, ICommand> handlers = new Dictionary<string, ICommand>();
            void addHandler(ICommand handler) {
                handlers.Add(handler.CommandName, handler);
            };

            // RFC 1459
            addHandler(new AdminCommand());
            addHandler(new AwayCommand());
            addHandler(new InfoCommand());
            addHandler(new InviteCommand());
            addHandler(new IsOnCommand());
            addHandler(new JoinCommand());
            addHandler(new KickCommand());
            addHandler(new KillCommand());
            addHandler(new ListCommand());
            addHandler(new ListUsersCommand());
            addHandler(new MessageOfTheDayCommand());
            addHandler(new ModeCommand());
            addHandler(new NamesCommand());
            addHandler(new NickCommand());
            addHandler(new NoticeCommand());
            addHandler(new PartCommand());
            addHandler(new PassCommand());
            addHandler(new PingCommand());
            addHandler(new PrivateMessageCommand());
            addHandler(new QuitCommand());
            addHandler(new StatsCommand());
            addHandler(new SummonCommand());
            addHandler(new TimeCommand());
            addHandler(new TopicCommand());
            addHandler(new UserCommand());
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
            while(IsRunning) {
                try {
                    if(Socket.Poll(1000000, SelectMode.SelectRead)) {
                        IRCConnection conn = new IRCConnection(Socket.Accept());
                        Connections.Add(conn.Socket, conn);
                        conn.SendReply(new Replies.WelcomeReply());
                    }

                    if(Connections.Count < 1) {
                        Thread.Sleep(1000);
                        continue;
                    }

                    List<Socket> sockets = new List<Socket>(Connections.Keys);
                    Socket.Select(sockets, null, null, 5000000);

                    foreach(Socket sock in sockets) {
                        try {
                            int read = sock.Receive(Buffer);
                            string[] lines = Encoding.UTF8.GetString(Buffer, 0, read).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach(string line in lines)
                                OnReceive(Connections[sock], line);
                        } catch(SocketException ex) {
                            Logger.Write($@"[IRC] Socket Error: {ex}");
                        }
                    }

                    // check pings
                } catch(Exception ex) {
                    Logger.Write($@"[IRC] {ex}");
                }
            }
        }

        private void OnReceive(IRCConnection conn, string line) {
            Logger.Debug($@"[{conn}] {line}");

            string prefix = null;
            string command = null;
            List<string> args = new List<string>();

            try {
                int i = 0;

                if(line[0] == PREFIX) {
                    while(line[++i] != ' ');
                    prefix = line[1..i];
                } else
                    prefix = @"meow";

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

            Logger.Debug($@"{prefix} {command} {string.Join(' ', args)}");

            if(Commands.ContainsKey(command))
                Commands[command].HandleCommand(new CommandArgs());
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

            IsRunning = false;
            Socket?.Dispose();
        }
    }
}
