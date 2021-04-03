using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class AdminCommand : ICommand {
        public const string NAME = @"ADMIN";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // return admin info
        }
    }
}
