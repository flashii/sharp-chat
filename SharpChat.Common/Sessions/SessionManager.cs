using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Protocol;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Sessions {
    public class SessionManager : IEventHandler {
        public const short DEFAULT_MAX_COUNT = 5;
        public const ushort DEFAULT_TIMEOUT = 5;

        private readonly object Sync = new object();

        private CachedValue<short> MaxPerUser { get; } 
        private CachedValue<ushort> TimeOut { get; }

        private IEventDispatcher Dispatcher { get; }
        private string ServerId { get; }

        private List<ISession> Sessions { get; } = new List<ISession>();
        private List<Session> LocalSessions { get; } = new List<Session>();

        public SessionManager(IEventDispatcher dispatcher, string serverId, IConfig config) {
            if(config == null)
                throw new ArgumentNullException(nameof(config));
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            MaxPerUser = config.ReadCached(@"maxCount", DEFAULT_MAX_COUNT);
            TimeOut = config.ReadCached(@"timeOut", DEFAULT_TIMEOUT);
        }
        
        public bool HasTimedOut(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            int timeOut = TimeOut;
            if(timeOut < 1) // avoid idiocy
                timeOut = DEFAULT_TIMEOUT;
            return session.GetIdleTime().TotalSeconds >= timeOut;
        }

        public void GetSession(Func<ISession, bool> predicate, Action<ISession> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                ISession session = Sessions.FirstOrDefault(predicate);
                if(session == null)
                    return;
                callback(session);
            }
        }

        public void GetSession(string sessionId, Action<ISession> callback) {
            if(sessionId == null)
                throw new ArgumentNullException(nameof(sessionId));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(string.IsNullOrWhiteSpace(sessionId)) {
                callback(null);
                return;
            }
            GetSession(s => sessionId.Equals(s.SessionId), callback);
        }

        public void GetSession(ISession session, Action<ISession> callback) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                // Check if we have a local session
                if(session is Session && LocalSessions.Contains(session)) {
                    callback(session);
                    return;
                }

                // Check if we're already an instance
                if(Sessions.Contains(session)) {
                    callback(session);
                    return;
                }

                // Finde
                GetSession(session.Equals, callback);
            }
        }

        public void GetLocalSession(Func<ISession, bool> predicate, Action<ISession> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(LocalSessions.FirstOrDefault(predicate));
        }

        public void GetLocalSession(ISession session, Action<ISession> callback) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                if(session is Session && LocalSessions.Contains(session)) {
                    callback(session);
                    return;
                }

                GetLocalSession(session.Equals, callback);
            }
        }

        public void GetLocalSession(string sessionId, Action<ISession> callback) {
            if(sessionId == null)
                throw new ArgumentNullException(nameof(sessionId));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetLocalSession(s => sessionId.Equals(s.SessionId), callback);
        }

        public void GetLocalSession(IConnection conn, Action<ISession> callback) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetLocalSession(s => s.HasConnection(conn), callback);
        }

        public void GetSessions(Func<ISession, bool> predicate, Action<IEnumerable<ISession>> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Sessions.Where(predicate));
        }

        public void GetSessions(IUser user, Action<IEnumerable<ISession>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetSessions(s => user.Equals(s.User), callback);
        }

        public void GetLocalSessions(Func<ISession, bool> predicate, Action<IEnumerable<ISession>> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(LocalSessions.Where(predicate));
        }

        public void GetLocalSessions(IUser user, Action<IEnumerable<ISession>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetLocalSessions(s => user.Equals(s.User), callback);
        }

        public void GetLocalSessions(IEnumerable<string> sessionIds, Action<IEnumerable<ISession>> callback) {
            if(sessionIds == null)
                throw new ArgumentNullException(nameof(sessionIds));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(!sessionIds.Any()) {
                callback(Enumerable.Empty<ISession>());
                return;
            }
            GetLocalSessions(s => sessionIds.Contains(s.SessionId), callback);
        }

        // i wonder what i'll think about this after sleeping a night on it
        // perhaps stick active sessions with the master User implementation again transparently.
        // session startups should probably be events as well
        public void GetSessions(IEnumerable<IUser> users, Action<IEnumerable<ISession>> callback) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetSessions(s => users.Any(s.User.Equals), callback);
        }

        public void GetLocalSessions(IEnumerable<IUser> users, Action<IEnumerable<ISession>> callback) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetLocalSessions(s => users.Any(s.User.Equals), callback);
        }

        public void GetActiveSessions(Action<IEnumerable<ISession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetSessions(s => !HasTimedOut(s), callback);
        }

        public void GetActiveLocalSessions(Action<IEnumerable<ISession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetLocalSessions(s => !HasTimedOut(s), callback);
        }

        public void GetDeadLocalSessions(Action<IEnumerable<ISession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetLocalSessions(HasTimedOut, callback);
        }

        public void Create(IConnection conn, IUser user, Action<ISession> callback) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            Session sess = null;

            lock(Sync) {
                sess = new Session(ServerId, conn, user);
                LocalSessions.Add(sess);
                Sessions.Add(sess);
            }

            Dispatcher.DispatchEvent(this, new SessionCreatedEvent(sess));
            callback(sess);
        }

        public void DoKeepAlive(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            lock(Sync)
                Dispatcher.DispatchEvent(this, new SessionPingEvent(session));
        }

        public void SwitchChannel(ISession session, IChannel channel = null) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            lock(Sync)
                Dispatcher.DispatchEvent(this, new SessionChannelSwitchEvent(channel, session));
        }

        public void Destroy(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));

            lock(Sync)
                GetLocalSession(conn, session => {
                    if(session == null)
                        return;
                    if(session is Session ls)
                        LocalSessions.Remove(ls);
                    Dispatcher.DispatchEvent(this, new SessionDestroyEvent(session));
                });
        }

        public void Destroy(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            lock(Sync)
                GetSession(session, session => {
                    if(session is Session ls)
                        LocalSessions.Remove(ls);

                    Dispatcher.DispatchEvent(this, new SessionDestroyEvent(session));
                });
        }

        public void HasSessions(IUser user, Action<bool> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Sessions.Any(s => user.Equals(s.User)));
        }

        public void GetSessionCount(IUser user, Action<int> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Sessions.Count(s => user.Equals(s.User)));
        }

        public void GetAvailableSessionCount(IUser user, Action<int> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetSessionCount(user, sessionCount => callback(MaxPerUser - sessionCount));
        }

        public void HasAvailableSessions(IUser user, Action<bool> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetAvailableSessionCount(user, availableSessionCount => callback(availableSessionCount > 0));
        }

        public void GetRemoteAddresses(IUser user, Action<IEnumerable<IPAddress>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            GetActiveSessions(sessions => {
                callback(sessions
                    .Where(s => user.Equals(s.User))
                    .OrderByDescending(s => s.LastPing)
                    .Select(s => s.RemoteAddress)
                    .Distinct());
            });
        }

        public void CheckTimeOut() {
            GetDeadLocalSessions(sessions => {
                if(sessions?.Any() != true)
                    return;

                Queue<ISession> murder = new Queue<ISession>(sessions);
                while(murder.TryDequeue(out ISession session))
                    Destroy(session);
            });
        }

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case SessionChannelSwitchEvent _:
                case SessionPingEvent _:
                case SessionResumeEvent _:
                case SessionSuspendEvent _:
                    GetSession(evt.SessionId, session => session?.HandleEvent(sender, evt));
                    break;

                case SessionCreatedEvent sce:
                    // sce needs to have sufficient info the create a session
                    break;

                case SessionDestroyEvent sde:
                    GetSession(sde.SessionId, session => {
                        Sessions.Remove(session);
                        session.HandleEvent(sender, sde);
                    });
                    break;
            }
        }
    }
}
