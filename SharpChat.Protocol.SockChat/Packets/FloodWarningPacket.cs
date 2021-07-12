using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class FloodWarningPacket : BotResponsePacket {
        public FloodWarningPacket(IUser sender)
            : base(sender.UserId, BotArguments.FLOOD_WARNING, false) { }
    }
}
