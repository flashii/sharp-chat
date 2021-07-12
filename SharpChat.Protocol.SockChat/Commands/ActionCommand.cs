using SharpChat.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class ActionCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name is @"action" or @"me";

        private MessageManager Messages { get; }

        public ActionCommand(MessageManager messages) {
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public bool DispatchCommand(CommandContext ctx) {
            if(ctx.Args.Count() < 2)
                return false;

            Messages.Create(ctx.Session, ctx.Channel, string.Join(' ', ctx.Args.Skip(1)), true);
            return true;
        }
    }
}
