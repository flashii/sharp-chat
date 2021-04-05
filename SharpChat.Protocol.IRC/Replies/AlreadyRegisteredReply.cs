namespace SharpChat.Protocol.IRC.Replies {
    public class AlreadyRegisteredReply : ServerReply {
        public const int CODE = 462;
        public const string LINE = @":You're already authenticated.";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
