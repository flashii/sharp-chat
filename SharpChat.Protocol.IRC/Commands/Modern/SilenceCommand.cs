using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.Modern {
    public class SilenceCommand : ICommand {
        public const string NAME = @"SILENCE";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // (un)silence people
        }
    }
}
