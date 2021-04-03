﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands.RFC1459 {
    public class TopicCommand : ICommand {
        public const string NAME = @"TOPIC";

        public string CommandName => NAME;

        public void HandleCommand(CommandArgs args) {
            // gets or sets a channel topic
        }
    }
}
