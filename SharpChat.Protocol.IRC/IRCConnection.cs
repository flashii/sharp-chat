using SharpChat.Protocol.IRC.Replies;
using SharpChat.Protocol.IRC.ServerCommands;
using SharpChat.Protocol.IRC.Users;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharpChat.Protocol.IRC {
    public class IRCConnection : IConnection {
        public const int ID_LENGTH = 16;

        public string ConnectionId { get; }
        public IPAddress RemoteAddress { get; }

        public bool IsAvailable => Socket.Connected;

        public Socket Socket { get; }
        private readonly object Sync = new object();

        public bool IsAuthenticating { get; set; }
        public bool HasAuthenticated { get; set; }
        public string Password { get; set; }

        public DateTimeOffset LastPing { get; set; }

        private IRCServer Server { get; }

        public ISession Session { get; set; }

        public IRCConnection(IRCServer server, Socket sock) {
            Socket = sock ?? throw new ArgumentNullException(nameof(sock));
            Server = server ?? throw new ArgumentNullException(nameof(server));
            ConnectionId = @"IRC!" + RNG.NextString(ID_LENGTH);
            RemoteAddress = sock.RemoteEndPoint is IPEndPoint ipep ? ipep.Address : IPAddress.None;
        }

        public void SendCommand(IServerCommand command) {
            StringBuilder sb = new StringBuilder();

            // Sender
            sb.Append(IRCServer.PREFIX);
            IUser sender = command.Sender;
            if(sender != null) {
                sb.Append(sender.GetIRCName());
                sb.Append('!');
                sb.Append(sender.UserName);
                sb.Append('@');
            }
            sb.Append(Server.ServerHost);
            sb.Append(' ');

            // Command
            sb.Append(command.CommandName);
            sb.Append(' ');

            // Contents
            sb.Append(command.GetLine());
            sb.Append(IServerCommand.CRLF);

            Send(sb);
        }

        public void SendReply(IReply reply) {
            StringBuilder sb = new StringBuilder();

            // Server
            sb.Append(IRCServer.PREFIX);
            sb.Append(Server.ServerHost);
            sb.Append(' ');

            // Reply code
            sb.AppendFormat(@"{0:000}", reply.ReplyCode);
            sb.Append(' ');

            // Receiver
            if(Session == null)
                sb.Append('-');
            else
                sb.Append(Session.User.GetIRCName());
            sb.Append(' ');

            // Contents
            sb.Append(reply.GetLine());
            sb.Append(IReply.CRLF);

            Send(sb);
        }

        public int Receive(byte[] buffer) {
            lock(Sync)
                return Socket.Receive(buffer);
        }

        private int Send(object obj) {
            lock(Sync)
                return Socket.Send(Encoding.UTF8.GetBytes(obj.ToString()));
        }

        public void Close() {
            lock(Sync) {
                Password = null;

                try {
                    Socket.Shutdown(SocketShutdown.Both);
                } finally {
                    Socket.Dispose();
                }
            }
        }

        public override string ToString()
            => $@"C#{ConnectionId}";
    }
}
