using SharpChat.Protocol.IRC.Replies;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class AdminCommand : IClientCommand {
        public const string NAME = @"ADMIN";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext ctx) {
            ctx.Connection.SendReply(new AdminMeReply());
            ctx.Connection.SendReply(new AdminLocation1Reply());
            ctx.Connection.SendReply(new AdminLocation2Reply());
            ctx.Connection.SendReply(new AdminEMailReply());
        }
    }
}
