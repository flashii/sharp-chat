using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Protocol.IRC.ServerCommands {
    public class ServerJoinCommand : ServerCommand {
        public const string NAME = @"JOIN";

        public override string CommandName => NAME;

        public override IUser Sender { get; }
        private IChannel Channel { get; }

        public ServerJoinCommand(IChannel channel, IUser user) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Sender = user ?? throw new ArgumentNullException(nameof(user));
        }

        protected override string BuildLine() {
            return Channel.GetIRCName();
        }
    }
}
