using SharpChat.Channels;
using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelDeleteResponsePacket : BotResponsePacket {
        public ChannelDeleteResponsePacket(IUser sender, string channelName)
            : base(sender, BotArguments.Notice(@"delchan", channelName)) { }

        public ChannelDeleteResponsePacket(IUser sender, IChannel channel)
            : this(sender, channel.Name) { }
    }
}
