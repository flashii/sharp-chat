namespace SharpChat.Protocol.IRC.Replies {
    public class PasswordMismatchReply : Reply {
        public const int CODE = 464;
        public const string LINE = @":Authentication failed.";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
