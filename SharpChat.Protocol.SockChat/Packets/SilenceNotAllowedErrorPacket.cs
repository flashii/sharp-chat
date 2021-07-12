using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilenceNotAllowedErrorPacket : BotResponsePacket {
        public SilenceNotAllowedErrorPacket(IUser sender)
            : base(sender, BotArguments.SILENCE_NOT_ALLOWED_ERROR, true) { }
    }
}
