using SharpChat.Channels;
using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelDeleteErrorPacket : BotResponsePacket {
        public ChannelDeleteErrorPacket(IUser sender, string channelName)
            : base(sender, BotArguments.CHANNEL_DELETE_ERROR, true, channelName) { }

        public ChannelDeleteErrorPacket(IUser sender, IChannel channel)
            : this(sender, channel.Name) { }
    }
}
