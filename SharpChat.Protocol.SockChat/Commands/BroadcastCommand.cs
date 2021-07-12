using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class BroadcastCommand : ICommand {
        private const string NAME = @"say";

        private Context Context { get; }
        private IUser Sender { get; }

        public BroadcastCommand(Context context, IUser sender) {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.Broadcast)) {
                ctx.Connection.SendPacket(new CommandNotAllowedErrorPacket(Sender, NAME));
                return true;
            }

            Context.BroadcastMessage(string.Join(' ', ctx.Args.Skip(1)));
            return true;
        }
    }
}
