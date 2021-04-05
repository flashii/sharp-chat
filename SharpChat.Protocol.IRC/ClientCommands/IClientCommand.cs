namespace SharpChat.Protocol.IRC.ClientCommands {
    public interface IClientCommand {
        string CommandName { get; }

        void HandleCommand(ClientCommandContext ctx);
    }
}
