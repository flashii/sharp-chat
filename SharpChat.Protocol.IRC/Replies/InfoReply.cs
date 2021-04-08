namespace SharpChat.Protocol.IRC.Replies {
    public class InfoReply : Reply {
        public const int CODE = 371;
        public const string LINE = @":Isn't it adorable when a spec marks something as REQUIRED but then gives no implementation details?";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
