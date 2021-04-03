using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class TimeCommand : ICommand {
        public const string NAME = @"TIME";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // returns local time
        }
    }
}
