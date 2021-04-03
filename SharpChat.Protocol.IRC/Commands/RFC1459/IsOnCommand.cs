using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class IsOnCommand : ICommand {
        public const string NAME = @"ISON";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // takes username list from args, and returns the ones that are online
        }
    }
}
