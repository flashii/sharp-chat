using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class InsufficientRankErrorPacket : BotResponsePacket {
        public InsufficientRankErrorPacket(IUser sender)
            : base(sender, BotArguments.INSUFFICIENT_RANK_ERROR, true) { }
    }
}
