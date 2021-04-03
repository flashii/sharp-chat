using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class SummonCommand : ICommand {
        public const string NAME = @"SUMMON";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // should inform that summon is disabled
        }
    }
}
