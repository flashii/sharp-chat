using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class PongPacket : ServerPacket {
        public DateTimeOffset PongTime { get; private set; }

        public PongPacket(SessionPingEvent spe) {
            PongTime = spe.DateTime;
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.Pong);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(PongTime.ToUnixTimeSeconds());

            return sb.ToString();
        }
    }
}
