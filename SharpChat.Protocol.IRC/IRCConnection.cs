using SharpChat.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
