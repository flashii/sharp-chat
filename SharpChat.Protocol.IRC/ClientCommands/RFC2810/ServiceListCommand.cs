using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC2810 {
    public class ServiceListCommand : IClientCommand {
        public const string NAME = @"SERVLIST";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // lists services, could be used for authentication but i think i'll just use the PASS field
            // not sure how i'm going to tackle auth entirely yet
        }
    }
}
