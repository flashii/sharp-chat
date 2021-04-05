using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class ClientCommandContext {
        public IRCConnection Connection { get; }
        public ISession Session { get; }
        public IEnumerable<string> Arguments { get; }

        public IUser User => Session.User;

        public bool HasSession => Session != null;
        public bool HasUser => HasSession;

        public ClientCommandContext(ISession session, IRCConnection connection, IEnumerable<string> args) {
            Session = session;
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Arguments = args ?? throw new ArgumentNullException(nameof(args));
        }
    }
}
