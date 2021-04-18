﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.Modern {
    public class CapabilitiesCommand : IClientCommand {
        public const string NAME = @"CAP";

        public string CommandName => NAME;
        public bool RequireSession => true;

        public void HandleCommand(ClientCommandContext args) {
            // capability shit
        }
    }
}
