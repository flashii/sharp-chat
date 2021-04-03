using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class NickCommand : ICommand {
        public const string NAME = @"NICK";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // set a nickname
        }
    }
}
