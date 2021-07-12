using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class CommandNotFoundErrorPacket : BotResponsePacket {
        public CommandNotFoundErrorPacket(IUser sender, string commandName)
            : base(sender, BotArguments.COMMAND_NOT_FOUND, true, commandName) { }
    }
}
