namespace SharpChat.Protocol.IRC.Replies {
    public class NoOperatorHostReply : IReply {
        public const int CODE = 491;
        public const string LINE = @":No O-lines for your host";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
