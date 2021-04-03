using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class FloodWarningPacket : BotResponsePacket {
        public FloodWarningPacket(IUser sender)
            : base(sender, BotArguments.Notice(@"flwarn")) { }
    }
}
