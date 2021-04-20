using SharpChat.Protocol.IRC.Users;
using SharpChat.Users;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class UserModeIsReply : Reply {
        public const int CODE = 221;

        public override int ReplyCode => CODE;

        private IUser User { get; }
        private bool IsSecure { get; }

        public UserModeIsReply(IUser user, bool isSecure) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            IsSecure = isSecure;
        }

        protected override string BuildLine() {
            return User.GetIRCModeString(IsSecure);
        }
    }
}
