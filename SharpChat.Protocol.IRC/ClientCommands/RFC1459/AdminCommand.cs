using SharpChat.Protocol.IRC.Replies;
using System;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class AdminCommand : IClientCommand {
        public const string NAME = @"ADMIN";

        public string CommandName => NAME;
        public bool RequireSession => true;

        private IRCServer Server { get; }

        public AdminCommand(IRCServer server) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            ctx.Connection.SendReply(new AdminMeReply());
            ctx.Connection.SendReply(new AdminLocation1Reply());
            ctx.Connection.SendReply(new AdminLocation2Reply());
            ctx.Connection.SendReply(new AdminEMailReply());
        }
    }
}
