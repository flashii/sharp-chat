using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class DeleteMessageNotFoundErrorPacket : BotResponsePacket {
        public DeleteMessageNotFoundErrorPacket(IUser sender)
            : base(sender, BotArguments.DELETE_MESSAGE_NOT_FOUND_ERROR, true) { }
    }
}
