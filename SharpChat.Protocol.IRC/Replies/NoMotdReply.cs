namespace SharpChat.Protocol.IRC.Replies {
    public class NoMotdReply : ServerReply {
        public const int CODE = 422;
        public const string LINE = @":There is no MOTD";

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            return LINE;
        }
    }
}
