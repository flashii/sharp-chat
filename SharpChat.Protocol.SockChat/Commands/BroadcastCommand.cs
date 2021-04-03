using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class BroadcastCommand : ICommand {
        private const string NAME = @"say";

        private Context Context { get; }

        public BroadcastCommand(Context context) {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.Broadcast))
                throw new CommandNotAllowedException(NAME);

            Context.BroadcastMessage(string.Join(' ', ctx.Args.Skip(1)));
            return true;
        }
    }
}
