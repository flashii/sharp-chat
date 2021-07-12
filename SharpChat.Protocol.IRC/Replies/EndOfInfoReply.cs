namespace SharpChat.Protocol.IRC.Replies {
    public class EndOfInfoReply : IReply {
        public const int CODE = 374;
        public const string LINE = @":End of INFO list";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
