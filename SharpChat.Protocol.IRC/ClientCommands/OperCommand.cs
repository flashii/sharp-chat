using SharpChat.Protocol.IRC.Replies;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class OperCommand : IClientCommand {
        public const string NAME = @"OPER";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext ctx) {
            ctx.Connection.SendReply(
                ctx.Arguments.Count() < 2
                    ? new NeedMoreParamsReply(NAME)
                    : new NoOperatorHostReply()
            );
        }
    }
}
