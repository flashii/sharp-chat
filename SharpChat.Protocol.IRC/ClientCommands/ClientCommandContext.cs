using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class ClientCommandContext {
        public IRCConnection Connection { get; }
        public IEnumerable<string> Arguments { get; }

        public ISession Session => Connection.Session;
        public IUser User => Session?.User;

        public ClientCommandContext(IRCConnection connection, IEnumerable<string> args) {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Arguments = args ?? throw new ArgumentNullException(nameof(args));
        }
    }
}
