using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;

namespace SharpChat.Protocol.IRC.ServerCommands {
    public class ServerJoinCommand : ServerCommand {
        public const string NAME = @"JOIN";

        public override string CommandName => NAME;

        private IChannel Channel { get; }

        public ServerJoinCommand(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return Channel.GetIRCName();
        }
    }
}
