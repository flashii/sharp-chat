using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class UserHostCommand : ICommand {
        public const string NAME = @"USERHOST";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // returns information about users
        }
    }
}
