using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Protocol.IRC.ServerCommands {
    public class ServerPartCommand : ServerCommand {
        public const string NAME = @"PART";

        public override string CommandName => NAME;

        public override IUser Sender { get; }
        private IChannel Channel { get; }
        private UserDisconnectReason Reason { get; }

        public ServerPartCommand(IChannel channel, IUser user, UserDisconnectReason reason) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Sender = user ?? throw new ArgumentNullException(nameof(user));
            Reason = reason;
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :{Reason}";
        }
    }
}
