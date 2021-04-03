using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class WhoWasCommand : ICommand {
        public const string NAME = @"WHOWAS";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // returns the obituary of a deceased user
        }
    }
}
