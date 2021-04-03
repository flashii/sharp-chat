using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class WAllOpsCommand : ICommand { // think it's Supposed to be Warn ALL OPS, using this to substitute /say
        public const string NAME = @"WALLOPS";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            //
        }
    }
}
