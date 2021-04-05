using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class ListUsersCommand : IClientCommand {
        public const string NAME = @"LUSERS";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // returns server user stats
        }
    }
}
