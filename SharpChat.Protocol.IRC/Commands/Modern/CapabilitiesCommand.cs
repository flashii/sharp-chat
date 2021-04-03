using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.Modern {
    public class CapabilitiesCommand : ICommand {
        public const string NAME = @"CAP";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // capability shit
        }
    }
}
