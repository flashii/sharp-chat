using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class CommandFormatErrorPacket : BotResponsePacket {
        public CommandFormatErrorPacket(IUser sender)
            : base(sender, BotArguments.COMMAND_FORMAT_ERROR, true) { }
    }
}
