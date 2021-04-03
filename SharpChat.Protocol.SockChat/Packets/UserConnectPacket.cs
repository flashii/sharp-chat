using SharpChat.Events;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserConnectPacket : ServerPacket {
        private UserConnectEvent Connect { get; }

        public UserConnectPacket(UserConnectEvent connect) {
            Connect = connect ?? throw new ArgumentNullException(nameof(connect));
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.UserConnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Connect.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Connect.User.Pack());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Connect.EventId);

            return sb.ToString();
        }
    }
}
