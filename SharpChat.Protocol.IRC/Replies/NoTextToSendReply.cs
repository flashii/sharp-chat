namespace SharpChat.Protocol.IRC.Replies {
    public class NoTextToSendReply : Reply {
        public const int CODE = 412;
        public const string LINE = @":No text to send";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
