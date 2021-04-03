using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class MessageDeletePacket : ServerPacket {
        public long MessageId { get; }

        public MessageDeletePacket(MessageUpdateEvent mue) {
            MessageId = (mue ?? throw new ArgumentNullException(nameof(mue))).MessageId;
        }

        public MessageDeletePacket(MessageDeleteEvent mde) {
            MessageId = (mde ?? throw new ArgumentNullException(nameof(mde))).MessageId;
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.MessageDelete);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(MessageId);

            return sb.ToString();
        }
    }
}
