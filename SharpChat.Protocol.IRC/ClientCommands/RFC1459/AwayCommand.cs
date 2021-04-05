using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class AwayCommand : IClientCommand {
        public const string NAME = @"AWAY";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // set away status
        }
    }
}
