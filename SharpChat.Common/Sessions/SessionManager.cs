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

        public ISession GetSession(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            lock(Sync) {
                if(session is Session s && LocalSessions.Contains(s))
                    return s;
                if(Sessions.Contains(session))
                    return session;
                return Sessions.FirstOrDefault(s => s.Equals(session));
            }
        }

        public ISession GetSession(string serverId, string sessionId) {
            if(serverId == null)
                throw new ArgumentNullException(nameof(serverId));
            if(sessionId == null)
                throw new ArgumentNullException(nameof(sessionId));

            lock(Sync)
                return Sessions.FirstOrDefault(s => serverId.Equals(s.ServerId) && sessionId.Equals(s.SessionId));
        }

        public ISession GetLocalSession(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            lock(Sync) {
                if(session is Session s && LocalSessions.Contains(s))
                    return s;
                return LocalSessions.FirstOrDefault(s => s.Equals(session));
            }
        }

        public ISession GetLocalSession(string sessionId) {
            if(sessionId == null)
                throw new ArgumentNullException(nameof(sessionId));

            lock(Sync)
                return LocalSessions.FirstOrDefault(s => sessionId.Equals(s.SessionId));
        }

        public ISession GetLocalSession(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));

            lock(Sync)
                return LocalSessions.FirstOrDefault(s => s.HasConnection(conn));
        }

        public void GetSessions(IUser user, Action<IEnumerable<ISession>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(Sessions.Where(s => user.Equals(s.User)));
        }

        public void GetLocalSessions(IUser user, Action<IEnumerable<ISession>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(LocalSessions.Where(s => user.Equals(s.User)));
        }

        // i wonder what i'll think about this after sleeping a night on it
        // perhaps stick active sessions with the master User implementation again transparently.
        // session startups should probably be events as well
        public void GetSessions(IEnumerable<IUser> users, Action<IEnumerable<ISession>> callback) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(Sessions.Where(s => users.Any(s.User.Equals)));
        }

        public void GetLocalSessions(IEnumerable<IUser> users, Action<IEnumerable<ISession>> callback) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(LocalSessions.Where(s => users.Any(s.User.Equals)));
        }

        public void GetActiveSessions(Action<IEnumerable<ISession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(Sessions.Where(s => !HasTimedOut(s)));
        }

        public void GetActiveLocalSessions(Action<IEnumerable<ISession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(LocalSessions.Where(s => !HasTimedOut(s)));
        }

        public void GetDeadLocalSessions(Action<IEnumerable<ISession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(LocalSessions.Where(HasTimedOut));
        }

        public ISession Create(IConnection conn, IUser user) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            lock(Sync) {
                Session sess = new Session(ServerId, conn, user);
                LocalSessions.Add(sess);
                Sessions.Add(sess);
                Dispatcher.DispatchEvent(this, new SessionCreatedEvent(sess));
                return sess;
            }
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
                Dispatcher.DispatchEvent(this, new SessionChannelSwitchEvent(session, channel));
        }

        public void Destroy(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));

            lock(Sync) {
                ISession session = GetLocalSession(conn);
                if(session == null)
                    return;
                if(session is Session ls)
                    LocalSessions.Remove(ls);
                Dispatcher.DispatchEvent(this, new SessionDestroyEvent(session));
            }
        }

        public void Destroy(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            lock(Sync) {
                session = GetSession(session);
                if(session is Session ls)
                    LocalSessions.Remove(ls);

                Dispatcher.DispatchEvent(this, new SessionDestroyEvent(session));
            }
        }

        public bool HasSessions(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            lock(Sync)
                return Sessions.Any(s => user.Equals(s.User));
        }

        public int GetSessionCount(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            lock(Sync)
                return Sessions.Count(s => user.Equals(s.User));
        }

        public int GetAvailableSessionCount(IUser user) {
            return MaxPerUser - GetSessionCount(user);
        }

        public IEnumerable<IPAddress> GetRemoteAddresses(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            IEnumerable<IPAddress> addrs = Enumerable.Empty<IPAddress>();
            GetActiveSessions(sessions => {
                addrs = sessions.Where(s => s.User.Equals(user))
                                .OrderByDescending(s => s.LastPing)
                                .Select(s => s.RemoteAddress)
                                .Distinct();
            });
            return addrs;
        }

        public void CheckTimeOut() {
            lock(Sync) {
                IEnumerable<ISession> sessions = null;
                GetDeadLocalSessions(s => sessions = s.ToArray());
                if(sessions == null || !sessions.Any())
                    return;
                foreach(ISession session in sessions)
                    Destroy(session);
            }
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync) {
                switch(evt) {
                    case SessionDestroyEvent sde:
                        Sessions.RemoveAll(s => sde.SessionId.Equals(s.SessionId));
                        break;
                }

                if(evt is SessionEvent se)
                    GetLocalSession(se.SessionId)?.HandleEvent(sender, se);
            }
        }
    }
}
