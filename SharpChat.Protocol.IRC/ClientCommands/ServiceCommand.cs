using SharpChat.Protocol.IRC.Replies;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class ServiceCommand : IClientCommand {
        public const string NAME = @"SERVICE";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext ctx) {
            ctx.Connection.SendReply(new AlreadyRegisteredReply());
        }
    }
}
