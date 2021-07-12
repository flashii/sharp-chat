using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilenceResponsePacket : BotResponsePacket {
        public SilenceResponsePacket(IUser sender, string userName)
            : base(sender, BotArguments.SILENCE_PLACE_CONFIRM, false, userName) { }

        public SilenceResponsePacket(IUser sender, IUser target)
            : this(sender, target.GetDisplayName()) { }
    }
}
