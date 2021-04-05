using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC2810 {
    public class ServiceQueryCommand : IClientCommand {
        public const string NAME = @"SQUERY";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // identical to PRIVMSG but ensures receiver is a service
        }
    }
}
