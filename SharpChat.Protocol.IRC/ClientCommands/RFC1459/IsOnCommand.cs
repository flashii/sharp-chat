using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class IsOnCommand : IClientCommand {
        public const string NAME = @"ISON";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // takes username list from args, and returns the ones that are online
        }
    }
}
