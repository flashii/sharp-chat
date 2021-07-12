using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol {
    public class ConnectionList<TConnection>
        where TConnection : IConnection {
        private HashSet<TConnection> Connections { get; } = new();
        private readonly object Sync = new();

        private ChannelUserRelations ChannelUsers { get; }

        public ConnectionList(ChannelUserRelations channelUsers) {
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
        }

        public virtual void AddConnection(TConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sync)
                Connections.Add(conn);
        }

        public virtual void RemoveConnection(TConnection conn) {
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

        public void GetConnectionBySessionId(string sessionId, Action<TConnection> callback) {
            if(sessionId == null)
                throw new ArgumentNullException(nameof(sessionId));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(string.IsNullOrWhiteSpace(sessionId)) {
                callback(default);
                return;
            }
            GetConnection(c => c.Session != null && sessionId.Equals(c.Session.SessionId), callback);
        }

        public void GetConnections(Func<TConnection, bool> predicate, Action<IEnumerable<TConnection>> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Connections.Where(predicate));
        }

        public void GetConnectionsWithSession(Action<IEnumerable<TConnection>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetConnections(c => c.Session != null, callback);
        }

        public void GetOwnConnections(IUser user, Action<IEnumerable<TConnection>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetConnections(c => c.Session != null && user.Equals(c.Session.User), callback);
        }

        public void GetConnectionsByChannelId(string channelId, Action<IEnumerable<TConnection>> callback) {
            if(channelId == null)
                throw new ArgumentNullException(nameof(channelId));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            ChannelUsers.GetLocalSessionsByChannelId(channelId, sessions => GetConnections(sessions, callback));
        }

        public void GetConnectionsByChannelName(string channelName, Action<IEnumerable<TConnection>> callback) {
            if(channelName == null)
                throw new ArgumentNullException(nameof(channelName));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            ChannelUsers.GetLocalSessionsByChannelName(channelName, sessions => GetConnections(sessions, callback));
        }

        public void GetConnections(IChannel channel, Action<IEnumerable<TConnection>> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            ChannelUsers.GetLocalSessions(channel, sessions => GetConnections(sessions, callback));
        }

        public void GetConnections(IEnumerable<ISession> sessions, Action<IEnumerable<TConnection>> callback) {
            if(sessions == null)
                throw new ArgumentNullException(nameof(sessions));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(!sessions.Any()) {
                callback(Enumerable.Empty<TConnection>());
                return;
            }
            lock(Sync)
                callback(sessions.Where(s => s.Connection is TConnection conn && Connections.Contains(conn)).Select(s => (TConnection)s.Connection));
        }

        public void GetAllConnections(IUser user, Action<IEnumerable<TConnection>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            ChannelUsers.GetLocalSessions(user, sessions => GetConnections(sessions, callback));
        }

        public void GetAllConnectionsByUserId(long userId, Action<IEnumerable<TConnection>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(userId < 1) {
                callback(Enumerable.Empty<TConnection>());
                return;
            }
            ChannelUsers.GetLocalSessionsByUserId(userId, sessions => GetConnections(sessions, callback));
        }

        public void GetDeadConnections(Action<IEnumerable<TConnection>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetConnections(c => !c.IsAvailable, callback);
        }
    }
}
