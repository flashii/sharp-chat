﻿using SharpChat.Events;
using System.Collections.Generic;

namespace SharpChat.Commands {
    public interface ICommand {
        bool IsCommandMatch(string name, IEnumerable<string> args);
        MessageCreateEvent DispatchCommand(ICommandContext ctx);
    }
}
