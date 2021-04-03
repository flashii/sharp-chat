namespace SharpChat.Protocol.IRC.Replies {
    public interface IServerReply {
        public const string CRLF = "\r\n";

        int ReplyCode { get; }

        string GetLine();
    }
}
