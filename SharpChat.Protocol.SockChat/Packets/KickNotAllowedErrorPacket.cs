using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class KickNotAllowedErrorPacket : BotResponsePacket {
        public KickNotAllowedErrorPacket(IUser sender, string userName)
            : base(sender, BotArguments.KICK_NOT_ALLOWED_ERROR, true, userName) { }
    }
}
