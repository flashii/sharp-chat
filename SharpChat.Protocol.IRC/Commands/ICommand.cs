using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Commands {
    public interface ICommand {
        string CommandName { get; }

        void HandleCommand(CommandArgs args);
    }
}
