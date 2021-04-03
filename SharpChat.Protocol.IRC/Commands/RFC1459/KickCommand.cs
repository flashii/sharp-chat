using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class KickCommand : ICommand {
        public const string NAME = @"KICK";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // kick a user from a channel
        }
    }
}
