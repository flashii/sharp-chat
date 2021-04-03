using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public enum ForceDisconnectReason {
        Kicked = 0,
        Banned = 1,
    }

    public class ForceDisconnectPacket : ServerPacket {
        public ForceDisconnectReason Reason { get; }
        public DateTimeOffset Expires { get; }
        public bool IsPermanent { get; }

        public ForceDisconnectPacket(ForceDisconnectReason reason, TimeSpan duration, bool isPermanent = false)
            : this(reason, DateTimeOffset.Now + duration, isPermanent) {}

        public ForceDisconnectPacket(ForceDisconnectReason reason, DateTimeOffset? expires = null, bool isPermanent = false) {
            Reason = reason;

            if (reason == ForceDisconnectReason.Banned) {
                if (!expires.HasValue)
                    throw new ArgumentNullException(nameof(expires));
                Expires = expires.Value;
                IsPermanent = isPermanent;
            }
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.BAKA);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)Reason);

            if (Reason == ForceDisconnectReason.Banned) {
                sb.Append(IServerPacket.SEPARATOR);
                if(IsPermanent)
                    sb.Append(-1);
                else
                    sb.Append((int)Expires.ToUnixTimeSeconds());
            }

            return sb.ToString();
        }
    }
}
