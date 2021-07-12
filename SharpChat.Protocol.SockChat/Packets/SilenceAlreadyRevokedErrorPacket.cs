using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilenceAlreadyRevokedErrorPacket : BotResponsePacket {
        public SilenceAlreadyRevokedErrorPacket(IUser sender)
            : base(sender, BotArguments.SILENCE_REVOKE_ALREADY_ERROR, true) { }
    }
}
