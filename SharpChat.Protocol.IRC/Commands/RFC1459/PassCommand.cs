using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class PassCommand : ICommand {
        public const string NAME = @"PASS";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // sets a password for the connection
        }
    }
}
