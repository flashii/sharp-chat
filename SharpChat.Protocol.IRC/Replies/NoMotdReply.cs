namespace SharpChat.Protocol.IRC.Replies {
    public class NoMotdReply : IReply {
        public const int CODE = 422;
        public const string LINE = @":There is no MOTD";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
