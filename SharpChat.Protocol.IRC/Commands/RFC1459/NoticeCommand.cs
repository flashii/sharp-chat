using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class NoticeCommand : ICommand {
        public const string NAME = @"NOTICE";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // like privmsg but autoreplies should not be sent
            // should this be supported?
        }
    }
}
