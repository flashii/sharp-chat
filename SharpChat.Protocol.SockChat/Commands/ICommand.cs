using System.Collections.Generic;

namespace SharpChat.Protocol.SockChat.Commands {
    public interface ICommand {
        bool IsCommandMatch(string name, IEnumerable<string> args);
        bool DispatchCommand(CommandContext ctx);
    }
}
