using SharpChat.Events;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Users {
    public static class IUserExtensions {
        public static string GetDisplayName(this IUser user) {
            if(user is ChatBot)
                return user.UserName;

            StringBuilder sb = new StringBuilder();

            if(user.Status == UserStatus.Away)
                sb.Append(user.StatusMessage.ToAFKString());

            if(string.IsNullOrWhiteSpace(user.NickName))
                sb.Append(user.UserName);
            else {
                sb.Append('~');
                sb.Append(user.NickName);
            }

            return sb.ToString();
        }

        public static string GetDisplayName(this UserUpdateEvent uue) {
            StringBuilder sb = new StringBuilder();

            if((uue.NewStatus ?? uue.OldStatus) == UserStatus.Away)
                sb.Append((uue.NewStatusMessage ?? uue.OldStatusMessage).ToAFKString());

            if(string.IsNullOrWhiteSpace(uue.NewNickName ?? uue.OldNickName))
                sb.Append(uue.NewUserName ?? uue.OldUserName);
            else {
                sb.Append('~');
                sb.Append(uue.NewNickName ?? uue.OldNickName);
            }

            return sb.ToString();
        }

        public static string ToAFKString(this string str)
            => string.Format(@"&lt;{0}&gt;_", str.Substring(0, Math.Min(str.Length, 5)).ToUpperInvariant());

        public static string Pack(this IUser user) {
            StringBuilder sb = new StringBuilder();
            user.Pack(sb);
            return sb.ToString();
        }

        public static void Pack(this IUser user, StringBuilder sb) {
            sb.Append(user.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(user.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(user.Colour);
            sb.Append(IServerPacket.SEPARATOR);

            if(!user.IsBot()) { // permission part is empty for bot apparently
                sb.Append(user.Rank);
                sb.Append(' ');
                sb.Append(user.Can(UserPermissions.KickUser) ? '1' : '0');
                sb.Append(@" 0 ");
                sb.Append(user.Can(UserPermissions.SetOwnNickname) ? '1' : '0');
                sb.Append(' ');
                sb.Append(user.Can(UserPermissions.CreateChannel | UserPermissions.SetChannelPermanent) ? 2 : (
                    user.Can(UserPermissions.CreateChannel) ? 1 : 0
                ));
            }
        }

        public static void Pack(this UserPermissions perms, StringBuilder sb) {
            sb.Append(' ');
            sb.Append((perms & UserPermissions.KickUser) > 0 ? '1' : '0');
            sb.Append(' ');
            sb.Append(0); // Legacy view logs
            sb.Append(' ');
            sb.Append((perms & UserPermissions.SetOwnNickname) > 0 ? '1' : '0');
            sb.Append(' ');
            sb.Append((perms & UserPermissions.CreateChannel) > 0 ? ((perms & UserPermissions.SetChannelPermanent) > 0 ? 2 : 1) : 0);
        }

        public static string Pack(this UserPermissions perms) {
            StringBuilder sb = new StringBuilder();
            perms.Pack(sb);
            return sb.ToString();
        }
    }
}
