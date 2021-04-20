namespace SharpChat.Protocol.IRC.Replies {
    public class NoOperatorHostReply : Reply {
        public const int CODE = 491;
        public const string LINE = @":No O-lines for your host";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
