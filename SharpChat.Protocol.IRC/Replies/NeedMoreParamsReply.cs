using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class NeedMoreParamsReply : Reply {
        public const int CODE = 461;

        public override int ReplyCode => CODE;

        private string CommandName { get; }

        public NeedMoreParamsReply(string commandName) {
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
        }

        protected override string BuildLine() {
            return $@"{CommandName} :Not enough parameters";
        }
    }
}
