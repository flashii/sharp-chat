namespace SharpChat.Protocol.IRC.Replies {
    public class RestrictedReply : IReply {
        public const int CODE = 484;
        public const string LINE = @":Your connection is restricted!";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
