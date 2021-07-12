using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilenceRevokeNotAllowedErrorPacket : BotResponsePacket {
        public SilenceRevokeNotAllowedErrorPacket(IUser sender)
            : base(sender, BotArguments.SILENCE_REVOKE_NOT_ALLOWED_ERROR, true) { }
    }
}
