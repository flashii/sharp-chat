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
            sb.Append(Update.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            if(Update.Status == UserStatus.Away && Update.HasStatusMessage)
                sb.Append(Update.StatusMessage.ToAFKString());
            else if(Update.Status == UserStatus.Away)
                sb.Append(Update.User.StatusMessage.ToAFKString());
            if(Update.HasNickName) {
                sb.Append('~');
                sb.Append(Update.NickName);
            } else if(Update.HasUserName)
                sb.Append(Update.UserName);
            else
                sb.Append(Update.User.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.Colour ?? Update.User.Colour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Update.Rank ?? Update.User.Rank);
            (Update.Perms ?? Update.User.Permissions).Pack(sb);

            return sb.ToString();
        }
    }
}
