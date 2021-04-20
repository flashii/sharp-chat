using SharpChat.Protocol.IRC.Replies;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class PassCommand : IClientCommand {
        public const string NAME = @"PASS";

        public string CommandName => NAME;
        public bool RequireSession => false;

        public void HandleCommand(ClientCommandContext ctx) {
            if(ctx.Connection.HasAuthenticated) {
                ctx.Connection.SendReply(new AlreadyRegisteredReply());
                return;
            }

            string password = ctx.Arguments.FirstOrDefault();
            if(string.IsNullOrWhiteSpace(password)) {
                ctx.Connection.SendReply(new NeedMoreParamsReply(NAME));
                return;
            }

            ctx.Connection.Password = password;
        }
    }
}
