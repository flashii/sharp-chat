namespace SharpChat.Protocol.IRC.Replies {
    public class UserModeUnknownFlagReply : IReply {
        public const int CODE = 501;
        public const string LINE = @":Unknown MODE flag";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
