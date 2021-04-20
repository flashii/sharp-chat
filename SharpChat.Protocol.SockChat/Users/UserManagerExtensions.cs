using SharpChat.Users;
using System;

namespace SharpChat.Protocol.SockChat.Users {
    public static class UserManagerExtensions {
        public static void GetUserBySockChatName(this UserManager users, string userName, Action<IUser> callback) {
            if(userName == null)
                throw new ArgumentNullException(nameof(userName));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            users.GetUser(
                u => userName.Equals(u.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase)
                    || userName.Equals(u.UserName, StringComparison.InvariantCultureIgnoreCase)
                    || userName.Equals(u.NickName, StringComparison.InvariantCultureIgnoreCase),
                callback
            );
        }
    }
}
