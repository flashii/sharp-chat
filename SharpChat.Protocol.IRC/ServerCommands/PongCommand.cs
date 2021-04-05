using System;

namespace SharpChat.Protocol.IRC.ServerCommands {
    public class PongCommand : ServerCommand {
        public const string NAME = @"PONG";

        public override string CommandName => NAME;

        private string Argument { get; }

        public PongCommand(string argument) {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        protected override string BuildLine() {
            return Argument;
        }
    }
}
