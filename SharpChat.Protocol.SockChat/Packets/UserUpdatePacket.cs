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
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.UserUpdate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.UserId.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            if(Update.Status == UserStatus.Away && Update.HasStatusMessage)
                sb.Append(Update.StatusMessage.ToAFKString());
            else if(Update.Status == UserStatus.Away)
                sb.Append(Update.UserId.StatusMessage.ToAFKString());
            if(Update.HasNickName) {
                sb.Append('~');
                sb.Append(Update.NickName);
            } else if(Update.HasUserName)
                sb.Append(Update.UserName);
            else
                sb.Append(Update.UserId.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.Colour ?? Update.UserId.Colour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.Rank ?? Update.UserId.Rank);
            (Update.Perms ?? Update.UserId.Permissions).Pack(sb);

            return sb.ToString();
        }
    }
}
