using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilenceSelfErrorPacket : BotResponsePacket {
        public SilenceSelfErrorPacket(IUser sender)
            : base(sender, BotArguments.SILENCE_SELF_ERROR, true) { }
    }
}
