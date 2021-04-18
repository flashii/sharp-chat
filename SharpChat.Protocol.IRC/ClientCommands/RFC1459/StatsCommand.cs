using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class StatsCommand : IClientCommand {
        public const string NAME = @"STATS";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // returns server stats
        }
    }
}
