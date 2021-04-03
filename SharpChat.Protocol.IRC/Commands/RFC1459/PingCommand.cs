using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class PingCommand : ICommand {
        public const string NAME = @"PING";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // should reply with a pong
        }
    }
}
