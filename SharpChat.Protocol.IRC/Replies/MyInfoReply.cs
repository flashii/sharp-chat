using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class MyInfoReply : Reply {
        public const int CODE = 4;

        public override int ReplyCode => CODE;

        private IRCServer Server { get; }

        public MyInfoReply(IRCServer server) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        protected override string BuildLine() {
            return $@":{Server.ServerHost} {SharpInfo.ProgramName} ABCOaiowxz PTahklmnoqstvz";
        }
    }
}
