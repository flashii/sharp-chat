using SharpChat.Protocol.IRC.Users;
using SharpChat.Users;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class WelcomeReply : Reply {
        public const int CODE = 1;

        public override int ReplyCode => CODE;

        private IRCServer Server { get; }
        private IUser User { get; }

        public WelcomeReply(IRCServer server, IUser user) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        protected override string BuildLine() {
            // todo: allow customisation
            return $@":Welcome to {Server.NetworkName} IRC {User.GetIRCMask(Server)}";
        }
    }
}
