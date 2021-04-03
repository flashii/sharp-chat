namespace SharpChat.Users {
    public class ChatBot : IUser {
        public long UserId { get; } = -1;
        public string UserName { get; } = @"ChatBot";
        public Colour Colour { get; } = new Colour(0x9E8DA7);
        public int Rank { get; } = 0;
        public string NickName => null;
        public UserPermissions Permissions { get; } = (UserPermissions)(-1);
        public UserStatus Status => UserStatus.Online;
        public string StatusMessage => string.Empty;

        public bool Equals(IUser other)
            => other != null && (other is ChatBot || other.UserId == UserId);

        public override string ToString()
            => @"<ChatBot>";
    }
}
