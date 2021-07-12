using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.SockChat.Commands {
    public class LeaveCommand : ICommand {
        public const string NAME = @"leave";

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        private IUser Sender { get; }

        public LeaveCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.Connection.HasCapability(ClientCapability.MCHAN)) {
                ctx.Connection.SendPacket(new CommandNotFoundErrorPacket(Sender, NAME));
                return true;
            }

            // figure out the channel leaving logic
            // should i postpone this implementation till i have the event based shit in place?

            return true;
        }
    }
}
