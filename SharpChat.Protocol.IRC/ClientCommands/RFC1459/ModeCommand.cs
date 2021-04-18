using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class ModeCommand : IClientCommand {
        public const string NAME = @"MODE";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // sets modes on a user or a channel
        }
    }
}
