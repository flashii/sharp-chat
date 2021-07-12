using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserNotFoundPacket : BotResponsePacket {
        private const string FALLBACK = @"User";

        public UserNotFoundPacket(IUser sender, string userName)
            : base(sender, BotArguments.USER_NOT_FOUND_ERROR, true, userName ?? FALLBACK) { }
    }
}
