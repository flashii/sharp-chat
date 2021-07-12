namespace SharpChat.Protocol.IRC.Replies {
    public class MotdEndReply : IReply {
        public const int CODE = 376;
        public const string LINE = @":END of MOTD command";

        public int ReplyCode => CODE;

        public string GetLine() {
            return LINE;
        }
    }
}
