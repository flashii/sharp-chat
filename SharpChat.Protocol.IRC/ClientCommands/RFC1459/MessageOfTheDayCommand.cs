﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class MessageOfTheDayCommand : IClientCommand {
        public const string NAME = @"MOTD";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // returns the MOTD
        }
    }
}