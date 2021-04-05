namespace SharpChat.Protocol.IRC.Replies {
    public class EndOfInfoReply : ServerReply {
        public const int CODE = 374;
        public const string LINE = @":End of INFO list";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
