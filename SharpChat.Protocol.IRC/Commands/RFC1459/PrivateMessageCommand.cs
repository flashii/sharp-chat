using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class PrivateMessageCommand : ICommand {
        public const string NAME = @"PRIVMSG";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // sends a message to a channel or a user
        }
    }
}
