using SharpChat.Events;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserConnectPacket : ServerPacket {
        private UserConnectEvent Connect { get; }
        private IUser User { get; }

        public UserConnectPacket(UserConnectEvent connect, IUser user) {
            Connect = connect ?? throw new ArgumentNullException(nameof(connect));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.UserConnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Connect.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.Pack());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Connect.EventId);

            return sb.ToString();
        }
    }
}
