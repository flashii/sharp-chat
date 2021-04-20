using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class ServiceQueryCommand : IClientCommand {
        public const string NAME = @"SQUERY";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // identical to PRIVMSG but ensures receiver is a service
        }
    }
}
