using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class NoticeCommand : IClientCommand {
        public const string NAME = @"NOTICE";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // like privmsg but autoreplies should not be sent
            // should this be supported?
        }
    }
}
