using SharpChat.Events;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserDisconnectPacket : ServerPacket {
        private UserDisconnectEvent Disconnect { get; }
        private IUser User { get; }

        public UserDisconnectPacket(UserDisconnectEvent disconnect, IUser user) {
            Disconnect = disconnect ?? throw new ArgumentNullException(nameof(disconnect));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.UserDisconnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);

            switch(Disconnect.Reason) {
                case UserDisconnectReason.Leave:
                default:
                    sb.Append(@"leave");
                    break;
                case UserDisconnectReason.TimeOut:
                    sb.Append(@"timeout");
                    break;
                case UserDisconnectReason.Kicked:
                    sb.Append(@"kick");
                    break;
                case UserDisconnectReason.Flood:
                    sb.Append(@"flood");
                    break;
            }

            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Disconnect.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Disconnect.EventId);

            return sb.ToString();
        }
    }
}
