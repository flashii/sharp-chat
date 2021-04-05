using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class ClientCommandContext {
        public IRCConnection Connection { get; }
        public IEnumerable<string> Arguments { get; }

        public ClientCommandContext(IRCConnection connection, IEnumerable<string> args) {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Arguments = args ?? throw new ArgumentNullException(nameof(args));
        }
    }
}
