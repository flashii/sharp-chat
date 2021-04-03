using SharpChat.Channels;
using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelCreateResponsePacket : BotResponsePacket {
        public ChannelCreateResponsePacket(IUser sender, string channelName)
            : base(sender, BotArguments.Notice(@"crchan", channelName)) { }

        public ChannelCreateResponsePacket(IUser sender, IChannel channel)
            : this(sender, channel.Name) { }
    }
}
