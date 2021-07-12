using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserListChannelNotFoundPacket : BotResponsePacket {
        public UserListChannelNotFoundPacket(IUser sender, string channelName)
            : base(sender, BotArguments.USER_LIST_ERROR, true, channelName) { }
    }
}
