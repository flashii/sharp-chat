using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class JoinCommand : IClientCommand {
        public const string NAME = @"JOIN";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // join a channel
        }
    }
}
