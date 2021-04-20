using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.Sessions {
    public static class SessionManagerExtensions {
        public static void CheckIRCSecure(this SessionManager sessions, IUser user, Action<bool> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            sessions.GetSessions(user, sessions => callback(sessions.Any() && sessions.All(s => s.IsSecure)));
        }
    }
}
