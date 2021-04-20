using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Protocol.IRC.Users {
    public static class IUserExtensions {
        public static string GetIRCName(this IUser user, IChannel channel = null) {
            StringBuilder sb = new StringBuilder();

            // SharpChat does not implement per-channel permissions besides owner
            // if it did an IMember should exist
            // these should also be Less Hardcoded
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

        public static string GetIRCModeString(this IUser user, bool isSecure = false) {
            StringBuilder sb = new StringBuilder();

            // see GetIRCName for rank based modes

            if(user.UserId == 1) // flashwave
                sb.Append('A'); // administrator

            if(user.IsBot()) // bot, make IsBot attr
                sb.Append('B');

            if(user.Rank >= 10)
                sb.Append('C'); // co-admin

            if(user.Status == UserStatus.Away)
                sb.Append('a'); // away

            if(user.Status == UserStatus.Offline)
                sb.Append('i'); // invisible

            if(user.Rank < 10 && user.Rank >= 5)
                sb.Append('o'); // global mod

            sb.Append('w'); // wallops
            sb.Append('x'); // host hiding

            if(sessions != null && sessions.Any() && sessions.All(s => s.IsSecure))
                sb.Append('z'); // secure

            return sb.ToString();
        }
    }
}
