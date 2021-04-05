namespace SharpChat.Protocol.IRC.Replies {
    public class PasswordMismatchReply : ServerReply {
        public const int CODE = 464;
        public const string LINE = @":Authentication failed.";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
