using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class SilenceCommand : IClientCommand {
        public const string NAME = @"SILENCE";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // (un)silence people
        }
    }
}
