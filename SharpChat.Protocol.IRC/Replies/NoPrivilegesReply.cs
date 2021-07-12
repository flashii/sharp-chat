namespace SharpChat.Protocol.IRC.Replies {
    public class NoPrivilegesReply : IReply {
        public const int CODE = 481;
        public const string LINE = @":Permission Denied- You're not an IRC operator";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
