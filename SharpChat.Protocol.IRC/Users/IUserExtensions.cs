using SharpChat.Channels;
using SharpChat.Users;
using System.Text;

namespace SharpChat.Protocol.IRC.Users {
    public static class IUserExtensions {
        public static string GetIRCName(this IUser user, IChannel channel = null) {
            StringBuilder sb = new StringBuilder();

            // SharpChat does not implement per-channel permissions besides owner
            // if it did an IMember should exist
            if(channel != null) {
                if(user.UserId == 1) // flashwave
                    sb.Append('~');
                else if(user.Rank >= 10) // admins
                    sb.Append('&');
                else if(user.Rank >= 5) // mods
                    sb.Append('@');
                else if(user.UserId == channel.OwnerId) // channel owner
                    sb.Append('%');
                else if(user.Can(UserPermissions.SetOwnNickname)) // tenshi
                    sb.Append('+');
            }

            if(!string.IsNullOrWhiteSpace(user.NickName))
                sb.Append(user.NickName);
            else 
                sb.Append(user.UserName);

            return sb.ToString();
        }

        public static string GetIRCMask(this IUser user) {
            return $@"{user.GetIRCName()}!{user.UserName}";
        }

        public static string GetIRCMask(this IUser user, IRCServer server) {
            return $@"{user.GetIRCMask()}@{server.ServerHost}";
        }
    }
}
