using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelNameFormatErrorPacket : BotResponsePacket {
        public ChannelNameFormatErrorPacket(IUser sender)
            : base(sender, BotArguments.CHANNEL_NAME_FORMAT_ERROR, true) { }
    }
}
