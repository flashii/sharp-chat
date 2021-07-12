using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Packets {
    public class CommandNotAllowedErrorPacket : BotResponsePacket {
        public CommandNotAllowedErrorPacket(IUser sender, string commandName)
            : base(sender, BotArguments.COMMAND_NOT_ALLOWED, true, @"/" + commandName) { }

        public CommandNotAllowedErrorPacket(IUser sender, IEnumerable<string> argList)
            : this(sender, argList.First()) { }
    }
}
