using SharpChat.Events;
using SharpChat.Protocol.SockChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelJoinPacket : ServerPacket {
        private ChannelUserJoinEvent Join { get; }

        public ChannelJoinPacket(ChannelUserJoinEvent join) {
            Join = join ?? throw new ArgumentNullException(nameof(join));
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMoveSubPacketId.UserJoined);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.UserId.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.UserId.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.UserId.Colour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.EventId);

            return sb.ToString();
        }
    }
}
