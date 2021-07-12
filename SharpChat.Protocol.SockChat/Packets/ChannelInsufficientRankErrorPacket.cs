using SharpChat.Channels;
using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelInsufficientRankErrorPacket : BotResponsePacket {
        public ChannelInsufficientRankErrorPacket(IUser sender, string channelName)
            : base(sender, BotArguments.CHANNEL_INSUFFICIENT_RANK_ERROR, true, channelName) { }

        public ChannelInsufficientRankErrorPacket(IUser sender, IChannel channel)
            : this(sender, channel.Name) { }
    }
}
