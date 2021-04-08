using System;

namespace SharpChat.Protocol.IRC.ServerCommands {
    public class ServerPongCommand : ServerCommand {
        public const string NAME = @"PONG";

        public override string CommandName => NAME;

        private IRCServer Server { get; }
        private string Argument { get; }

        public ServerPongCommand(IRCServer server, string argument) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        protected override string BuildLine() {
            return $@"{IRCServer.PREFIX}{Server.Name} {Argument}";
        }
    }
}
