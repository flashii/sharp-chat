﻿using SharpChat.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class ActionCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"action" || name == @"me";

        private MessageManager Messages { get; }

        public ActionCommand(MessageManager messages) {
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public bool DispatchCommand(ICommandContext ctx) {
            if(ctx.Args.Count() < 2)
                return false;

            Messages.Create(ctx.User, ctx.Channel, string.Join(' ', ctx.Args.Skip(1)), true);
            return true;
        }
    }
}
