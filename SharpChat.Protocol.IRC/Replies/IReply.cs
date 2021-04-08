namespace SharpChat.Protocol.IRC.Replies {
    public interface IReply {
        public const string CRLF = "\r\n";

        int ReplyCode { get; }

        string GetLine();
    }
}
