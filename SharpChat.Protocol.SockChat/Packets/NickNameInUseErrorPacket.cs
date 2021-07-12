using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class NickNameInUseErrorPacket : BotResponsePacket {
        public NickNameInUseErrorPacket(IUser sender, string nickName)
            : base(sender, BotArguments.NICKNAME_IN_USE_ERROR, true, nickName) { }
    }
}
