using SharpChat.Channels;
using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelAlreadyJoinedErrorPacket : BotResponsePacket {
        public ChannelAlreadyJoinedErrorPacket(IUser sender, string channelName)
            : base(sender, BotArguments.CHANNEL_ALREADY_JOINED_ERROR, true, channelName) { }

        public ChannelAlreadyJoinedErrorPacket(IUser sender, IChannel channel)
            : this(sender, channel.Name) { }
    }
}
