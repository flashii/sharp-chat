namespace SharpChat.Protocol.IRC.Replies {
    public class AlreadyRegisteredReply : IReply {
        public const int CODE = 462;
        public const string LINE = @":You're already authenticated.";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
