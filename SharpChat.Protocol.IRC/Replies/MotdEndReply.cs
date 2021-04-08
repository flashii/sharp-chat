namespace SharpChat.Protocol.IRC.Replies {
    public class MotdEndReply : Reply {
        public const int CODE = 376;
        public const string LINE = @":END of MOTD command";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
