using SharpChat.Protocol.IRC.Replies;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class PassCommand : IClientCommand {
        public const string NAME = @"PASS";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext ctx) {
            if(ctx.Connection.HasAuthenticated) {
                ctx.Connection.SendReply(new AlreadyRegisteredReply());
                return;
            }

            ctx.Connection.Password = ctx.Arguments.FirstOrDefault();
        }
    }
}
