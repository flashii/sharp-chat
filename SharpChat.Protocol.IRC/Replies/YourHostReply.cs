using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class YourHostReply : Reply {
        public const int CODE = 2;

        public override int ReplyCode => CODE;

        private IRCServer Server { get; }

        public YourHostReply(IRCServer server) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        protected override string BuildLine() {
            return $@":Your host is {Server.ServerHost}, running version {SharpInfo.ProgramName}";
        }
    }
}
