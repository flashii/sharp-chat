namespace SharpChat.Protocol.IRC.ServerCommands {
    public interface IServerCommand {
        public const string CRLF = "\r\n";

        string CommandName { get; }

        string GetLine();
    }
}
