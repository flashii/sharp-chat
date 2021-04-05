using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class AdminCommand : IClientCommand {
        public const string NAME = @"ADMIN";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // return admin info
        }
    }
}
