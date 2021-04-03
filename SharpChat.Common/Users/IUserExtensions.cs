namespace SharpChat.Users {
    public static class IUserExtensions {
        public static bool IsBot(this IUser user)
            => user is ChatBot || user?.UserId == -1;

        public static bool Can(this IUser user, UserPermissions perm)
            => user.IsBot() || (user.Permissions & perm) == perm;
    }
}
