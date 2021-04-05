using SharpChat.Protocol.IRC.ServerCommands;
using SharpChat.Sessions;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class PingCommand : IClientCommand {
        public const string NAME = @"PING";

        public string CommandName => NAME;

        private SessionManager Sessions { get; }

        public PingCommand(SessionManager sessions) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            if(ctx.HasSession && ctx.Arguments.Any()) { // only process pings when we have a session
                Sessions.DoKeepAlive(ctx.Session);
                ctx.Connection.SendCommand(new ServerPongCommand(ctx.Arguments.FirstOrDefault()));
            }
        }
    }
}
