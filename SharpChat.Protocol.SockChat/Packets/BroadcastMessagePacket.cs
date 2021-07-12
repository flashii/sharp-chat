using SharpChat.Events;

namespace SharpChat.Protocol.SockChat.Packets {
    public class BroadcastMessagePacket : BotResponsePacket {
        public BroadcastMessagePacket(BroadcastMessageEvent broadcast)
            : base(broadcast.UserId, BotArguments.BROADCAST, false, broadcast.Text) { }
    }
}
