using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class WhoWasCommand : IClientCommand {
        public const string NAME = @"WHOWAS";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // returns the obituary of a deceased user
        }
    }
}
