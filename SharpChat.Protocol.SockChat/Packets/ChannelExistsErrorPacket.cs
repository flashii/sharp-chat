using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelExistsErrorPacket : BotResponsePacket {
        public ChannelExistsErrorPacket(IUser sender, string channelName)
            : base(sender, BotArguments.CHANNEL_EXISTS_ERROR, true, channelName) { }
    }
}
