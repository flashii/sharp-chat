using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class WhoCommand : IClientCommand {
        public const string NAME = @"WHO";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // return info about users
        }
    }
}
