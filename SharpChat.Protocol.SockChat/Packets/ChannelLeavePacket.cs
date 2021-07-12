using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelLeavePacket : ServerPacket {
        private ChannelUserLeaveEvent Leave { get; }

        public ChannelLeavePacket(ChannelUserLeaveEvent leave) {
            Leave = leave ?? throw new ArgumentNullException(nameof(leave));
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMoveSubPacketId.UserLeft);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Leave.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Leave.EventId);

            return sb.ToString();
        }
    }
}
