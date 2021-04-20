using SharpChat.Protocol.IRC.Replies;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class InfoCommand : IClientCommand {
        public const string NAME = @"INFO";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext ctx) {
            ctx.Connection.SendReply(new InfoReply());
            ctx.Connection.SendReply(new EndOfInfoReply());
        }
    }
}
