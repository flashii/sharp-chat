using SharpChat.Events;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserUpdatePacket : ServerPacket {
        private UserUpdateEvent Update { get; }

        public UserUpdatePacket(UserUpdateEvent uue) {
            Update = uue ?? throw new ArgumentNullException(nameof(uue));
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.UserUpdate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.NewColour ?? Update.OldColour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.NewRank ?? Update.OldRank);
            (Update.NewPerms ?? Update.OldPerms).Pack(sb);

            return sb.ToString();
        }
    }
}
