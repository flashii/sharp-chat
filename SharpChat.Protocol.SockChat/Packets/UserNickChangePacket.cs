using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserNickChangePacket : BotResponsePacket {
        public UserNickChangePacket(IUser sender, string oldName, string newName)
            : base(sender, BotArguments.Notice(@"nick", oldName, newName)) { }
    }
}
