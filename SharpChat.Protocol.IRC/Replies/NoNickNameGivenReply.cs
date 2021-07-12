namespace SharpChat.Protocol.IRC.Replies {
    public class NoNickNameGivenReply : IReply {
        public const int CODE = 431;
        public const string LINE = @":No nickname given";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
