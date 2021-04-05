using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class NamesCommand : IClientCommand {
        public const string NAME = @"NAMES";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // lists users in channel, probably
        }
    }
}
