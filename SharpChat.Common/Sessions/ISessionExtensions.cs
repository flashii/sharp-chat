using System;

namespace SharpChat.Sessions {
    public static class ISessionExtensions {
        public static TimeSpan GetIdleTime(this ISession session)
            => session.LastPing - DateTimeOffset.Now;
    }
}
