using SharpChat.Events;
using SharpChat.Protocol.IRC.Replies;
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

        public IRCConnection(Socket sock) {
            Socket = sock ?? throw new ArgumentNullException(nameof(sock));
            ConnectionId = @"IRC!" + RNG.NextString(ID_LENGTH);
            RemoteAddress = sock.RemoteEndPoint is IPEndPoint ipep ? ipep.Address : IPAddress.None;
        }

        public void SendReply(IServerReply reply) {
            lock(Sync) {
                StringBuilder sb = new StringBuilder();
                sb.Append(IRCServer.PREFIX);
                sb.Append(@"irc.railgun.sh"); // server prefix
                sb.Append(' ');
                sb.AppendFormat(@"{0:000}", reply.ReplyCode);
                sb.Append(' ');
                sb.Append(@"flash"); // nickname, can be - sometimes but i don't know when
                sb.Append(' ');
                sb.Append(reply.GetLine());
                sb.Append(IServerReply.CRLF);

                Socket.Send(Encoding.UTF8.GetBytes(sb.ToString()));
            }
        }

        public void Close() {
            lock(Sync) {
                try {
                    Socket.Shutdown(SocketShutdown.Both);
                } finally {
                    Socket.Dispose();
                }
            }
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync) {
                //
            }
        }

        public override string ToString()
            => $@"C#{ConnectionId}";
    }
}
