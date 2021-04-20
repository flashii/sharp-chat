namespace SharpChat.Protocol.IRC.Replies {
    public class NoNickNameGivenReply : Reply {
        public const int CODE = 431;
        public const string LINE = @":No nickname given";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
