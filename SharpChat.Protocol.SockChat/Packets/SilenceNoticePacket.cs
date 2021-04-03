using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilenceNoticePacket : BotResponsePacket {
        public SilenceNoticePacket(IUser sender)
            : base(sender, BotArguments.Notice(@"silence")) {}
    }
}
