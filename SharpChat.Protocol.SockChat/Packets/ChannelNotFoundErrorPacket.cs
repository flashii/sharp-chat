using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelNotFoundErrorPacket : BotResponsePacket {
        public ChannelNotFoundErrorPacket(IUser sender, string channelName)
            : base(sender, BotArguments.CHANNEL_NOT_FOUND_ERROR, true, channelName) { }
    }
}
