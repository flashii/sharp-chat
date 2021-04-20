using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class NickNameInUseReply : Reply {
        public const int CODE = 433;

        public override int ReplyCode => CODE;

        private string NickName { get; }

        public NickNameInUseReply(string nickName) {
            NickName = nickName ?? throw new ArgumentNullException(nameof(nickName));
        }

        protected override string BuildLine() {
            return $@"{NickName} :Nickname is already in use";
        }
    }
}
