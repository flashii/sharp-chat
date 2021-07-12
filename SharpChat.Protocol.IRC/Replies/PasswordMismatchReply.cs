namespace SharpChat.Protocol.IRC.Replies {
    public class PasswordMismatchReply : IReply {
        public const int CODE = 464;
        public const string LINE = @":Authentication failed.";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
