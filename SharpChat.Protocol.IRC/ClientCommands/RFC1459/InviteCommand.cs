using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class InviteCommand : IClientCommand { // reintroduce this into Sock Chat
        public const string NAME = @"INVITE";

        public string CommandName => NAME;

        public void HandleCommand(ClientCommandContext args) {
            // invites another user to a channel
        }
    }
}
