using System;

namespace SharpChat.Sessions {
    public static class ISessionExtensions {
        public static bool HasCapability(this ISession session, ClientCapability capability)
            => (session.Capabilities & capability) == capability;

        public static TimeSpan GetIdleTime(this ISession session)
            => session.LastPing - DateTimeOffset.Now;
    }
}
