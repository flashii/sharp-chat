using System;

namespace SharpChat.Protocol.IRC.ServerCommands {
    public class ServerPongCommand : ServerCommand {
        public const string NAME = @"PONG";

        public override string CommandName => NAME;

        private string Argument { get; }

        public ServerPongCommand(string argument) {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        protected override string BuildLine() {
            return Argument;
        }
    }
}
