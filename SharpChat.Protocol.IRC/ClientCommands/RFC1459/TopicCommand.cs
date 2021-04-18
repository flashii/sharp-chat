using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class TopicCommand : IClientCommand {
        public const string NAME = @"TOPIC";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // gets or sets a channel topic
        }
    }
}
