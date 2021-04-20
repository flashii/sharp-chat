using SharpChat.Protocol.IRC.Replies;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class ServerQuitCommand : IClientCommand {
        public const string NAME = @"SQUIT";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext ctx) {
            ctx.Connection.SendReply(new NoPrivilegesReply());
        }
    }
}
