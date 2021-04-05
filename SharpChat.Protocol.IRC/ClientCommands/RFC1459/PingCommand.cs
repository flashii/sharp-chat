using SharpChat.Protocol.IRC.ServerCommands;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class PingCommand : IClientCommand {
        public const string NAME = @"PING";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext ctx) {
            if(ctx.Arguments.Any())
                ctx.Connection.SendCommand(new PongCommand(ctx.Arguments.FirstOrDefault()));
        }
    }
}
