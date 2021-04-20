namespace SharpChat.Protocol.IRC.Replies {
    public class RestrictedReply : Reply {
        public const int CODE = 484;
        public const string LINE = @":Your connection is restricted!";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
