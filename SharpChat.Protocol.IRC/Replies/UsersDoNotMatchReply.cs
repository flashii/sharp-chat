namespace SharpChat.Protocol.IRC.Replies {
    public class UsersDoNotMatchReply : Reply {
        public const int CODE = 502;
        public const string LINE = @":Cannot change mode for other users";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
