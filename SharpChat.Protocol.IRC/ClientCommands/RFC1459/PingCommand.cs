using SharpChat.Protocol.IRC.ServerCommands;
using SharpChat.Sessions;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class PingCommand : IClientCommand {
        public const string NAME = @"PING";

        public string CommandName => NAME;
        public bool RequireSession => true;

        private IRCServer Server { get; }
        private SessionManager Sessions { get; }

        public PingCommand(IRCServer server, SessionManager sessions) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            if(ctx.Arguments.Any()) {
                Sessions.DoKeepAlive(ctx.Session);
                ctx.Connection.SendCommand(new ServerPongCommand(Server, ctx.Arguments.FirstOrDefault()));
            }
        }
    }
}
