using SharpChat.Channels;
using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelInvalidPasswordErrorPacket : BotResponsePacket {
        public ChannelInvalidPasswordErrorPacket(IUser sender, string channelName)
            : base(sender, BotArguments.CHANNEL_INVALID_PASSWORD_ERROR, true, channelName) { }

        public ChannelInvalidPasswordErrorPacket(IUser sender, IChannel channel)
            : this(sender, channel.Name) { }
    }
}
