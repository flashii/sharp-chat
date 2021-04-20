using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class ErroneousNickNameReply : Reply {
        public const int CODE = 432;

        public override int ReplyCode => CODE;

        private string NickName { get; }

        public ErroneousNickNameReply(string nickName) {
            NickName = nickName ?? throw new ArgumentNullException(nameof(nickName));
        }

        protected override string BuildLine() {
            return $@"{NickName} :Erroneous nickname";
        }
    }
}
