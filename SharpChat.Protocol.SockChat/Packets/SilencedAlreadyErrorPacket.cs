using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilencedAlreadyErrorPacket : BotResponsePacket {
        public SilencedAlreadyErrorPacket(IUser sender)
            : base(sender, BotArguments.SILENCE_ALREADY_ERROR, true) { }
    }
}
