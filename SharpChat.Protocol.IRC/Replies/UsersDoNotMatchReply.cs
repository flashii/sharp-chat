namespace SharpChat.Protocol.IRC.Replies {
    public class UsersDoNotMatchReply : IReply {
        public const int CODE = 502;
        public const string LINE = @":Cannot change mode for other users";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
