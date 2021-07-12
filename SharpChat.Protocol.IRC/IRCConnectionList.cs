using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace SharpChat.Protocol.IRC {
    public class IRCConnectionList : ConnectionList<IRCConnection> {
        // it's not there if you don't look at it
        private Dictionary<Socket, IRCConnection> Connections { get; } = new();
        private Dictionary<IRCConnection, Socket> Sockets { get; } = new();
        private readonly object Sync = new();

        public IRCConnectionList(ChannelUserRelations channelUsers) : base(channelUsers) {
        }

        public override void AddConnection(IRCConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sync) {
                Connections.Add(conn.Socket, conn);
                Sockets.Add(conn, conn.Socket);
                base.AddConnection(conn);
            }
        }

        public override void RemoveConnection(IRCConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sync) {
                Connections.Remove(conn.Socket);
                Sockets.Remove(conn);
                base.RemoveConnection(conn);
            }
        }

        public void GetConnection(Socket sock, Action<IRCConnection> callback) {
            if(sock == null)
                throw new ArgumentNullException(nameof(sock));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Connections.TryGetValue(sock, out IRCConnection conn) ? conn : null);
        }

        public void GetReadyConnections(Action<IEnumerable<IRCConnection>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                if(!Connections.Any()) {
                    callback(Enumerable.Empty<IRCConnection>());
                    return;
                }

                List<Socket> sockets = new(Sockets.Values);
                Socket.Select(sockets, null, null, 5000000);
                callback(sockets.Select(s => Connections[s]));
            }
        }
    }
}
