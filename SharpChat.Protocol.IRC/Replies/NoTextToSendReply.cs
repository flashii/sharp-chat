namespace SharpChat.Protocol.IRC.Replies {
    public class NoTextToSendReply : IReply {
        public const int CODE = 412;
        public const string LINE = @":No text to send";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
