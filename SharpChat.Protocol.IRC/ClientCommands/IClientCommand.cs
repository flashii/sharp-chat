namespace SharpChat.Protocol.IRC.ClientCommands {
    public interface IClientCommand {
        string CommandName { get; }

        bool RequireSession { get; }

        void HandleCommand(ClientCommandContext ctx);
    }
}
