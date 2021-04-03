using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC2810 {
    public class ServiceListCommand : ICommand {
        public const string NAME = @"SERVLIST";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // lists services, could be used for authentication but i think i'll just use the PASS field
            // not sure how i'm going to tackle auth entirely yet
        }
    }
}
