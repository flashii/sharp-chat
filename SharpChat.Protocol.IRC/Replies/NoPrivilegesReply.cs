namespace SharpChat.Protocol.IRC.Replies {
    public class NoPrivilegesReply : Reply {
        public const int CODE = 481;
        public const string LINE = @":Permission Denied- You're not an IRC operator";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
