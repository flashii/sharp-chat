using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol {
    public class ConnectionList<TConnection>
        where TConnection : IConnection {
        private HashSet<TConnection> Connections { get; } = new HashSet<TConnection>();
        private readonly object Sync = new object();

        private ChannelUserRelations ChannelUsers { get; }

        public ConnectionList(ChannelUserRelations channelUsers) {
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
        }

        public void AddConnection(TConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sync)
                Connections.Add(conn);
        }

        public void RemoveConnection(TConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sync)
                Connections.Remove(conn);
        }

        public void RemoveConnection(string connId) {
            if(connId == null)
                throw new ArgumentNullException(nameof(connId));
            GetConnection(connId, c => Connections.Remove(c));
        }

        public void GetConnection(Func<TConnection, bool> predicate, Action<TConnection> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                TConnection conn = Connections.FirstOrDefault(predicate);
                if(conn == null)
                    return;
                callback(conn);
            }
        }

        public void GetConnection(string connId, Action<TConnection> callback) {
            if(connId == null)
                throw new ArgumentNullException(nameof(connId));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetConnection(c => connId.Equals(c.ConnectionId), callback);
        }

        public void GetConnection(ISession session, Action<TConnection> callback) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetConnection(c => session.Equals(c.Session), callback);
        }

        public void GetConnections(Func<TConnection, bool> predicate, Action<IEnumerable<TConnection>> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Connections.Where(predicate));
        }

        public void GetConnections(IUser user, Action<IEnumerable<TConnection>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetConnections(c => c.Session != null && user.Equals(c.Session.User), callback);
        }

        public void GetConnections(IChannel channel, Action<IEnumerable<TConnection>> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            //ChannelUsers.GetSessions(channel, );
        }
    }
}
