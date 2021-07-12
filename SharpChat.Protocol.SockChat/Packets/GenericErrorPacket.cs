using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class GenericErrorPacket : BotResponsePacket {
        public GenericErrorPacket(IUser sender)
            : base(sender, BotArguments.GENERIC_ERROR, true) { }
    }
}
