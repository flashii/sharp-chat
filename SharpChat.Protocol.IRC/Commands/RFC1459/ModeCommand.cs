using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class ModeCommand : ICommand {
        public const string NAME = @"MODE";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // sets modes on a user or a channel
        }
    }
}
