using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelPasswordResponsePacket : BotResponsePacket {
        public ChannelPasswordResponsePacket(IUser sender)
            : base(sender, BotArguments.CHANNEL_PASSWORD_CHANGED, false) { }
    }
}
