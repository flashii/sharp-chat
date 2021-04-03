using System.Collections.Generic;

namespace SharpChat.Protocol.SockChat.Commands {
    public class LeaveCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"leave";

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.Connection.HasCapability(ClientCapability.MCHAN))
                throw new CommandNotFoundException(@"leave");

            // figure out the channel leaving logic
            // should i postpone this implementation till i have the event based shit in place?

            return true;
        }
    }
}
