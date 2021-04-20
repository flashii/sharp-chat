namespace SharpChat.Protocol.IRC.Replies {
    public class UserModeUnknownFlagReply : Reply {
        public const int CODE = 501;
        public const string LINE = @":Unknown MODE flag";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
