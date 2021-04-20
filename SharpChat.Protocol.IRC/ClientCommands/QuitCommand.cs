using SharpChat.Sessions;
using System;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class QuitCommand : IClientCommand {
        public const string NAME = @"QUIT";

        public string CommandName => NAME;
        public bool RequireSession => false;

        private SessionManager Sessions { get; }

        public QuitCommand(SessionManager sessions) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            //string message = ctx.Arguments.ElementAtOrDefault(0);

            ctx.Connection.Close();

            if(ctx.Session != null)
                Sessions.Destroy(ctx.Connection);
        }
    }
}
