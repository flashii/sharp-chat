using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class KickCommand : IClientCommand {
        public const string NAME = @"KICK";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // kick a user from a channel
        }
    }
}
