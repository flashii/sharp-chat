namespace SharpChat.Protocol.IRC.Replies {
    public class ListEndReply : IReply {
        public const int CODE = 323;
        public const string LINE = @":End of LIST";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
