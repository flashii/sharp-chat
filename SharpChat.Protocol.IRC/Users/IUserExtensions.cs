using SharpChat.Users;

namespace SharpChat.Protocol.IRC.Users {
    public static class IUserExtensions {
        public static string GetIRCName(this IUser user) {
            if(!string.IsNullOrWhiteSpace(user.NickName))
                return user.NickName;
            return user.UserName;
        }
    }
}
