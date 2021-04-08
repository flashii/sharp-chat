namespace SharpChat.Protocol.IRC.Replies {
    public class ListEndReply : Reply {
        public const int CODE = 323;
        public const string LINE = @":End of LIST";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
