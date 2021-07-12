using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class NotBannedErrorPacket : BotResponsePacket {
        public NotBannedErrorPacket(IUser sender, string subject)
            : base(sender, BotArguments.NOT_BANNED_ERROR, true, subject) { }
    }
}
