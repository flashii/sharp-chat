using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class PrivateMessageCommand : IClientCommand {
        public const string NAME = @"PRIVMSG";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // sends a message to a channel or a user
        }
    }
}
