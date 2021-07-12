using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserNickChangePacket : BotResponsePacket {
        public UserNickChangePacket(IUser sender, string oldName, string newName)
            : base(sender, BotArguments.NICKNAME_CHANGE, false, oldName, newName) { }
    }
}
