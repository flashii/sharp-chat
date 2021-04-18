using SharpChat.Events;
using SharpChat.Users;

namespace SharpChat.Messages.Storage {
    public class MemoryMessageUser : IUser {
        public long UserId { get; }
        public string UserName { get; }
        public Colour Colour { get; }
        public int Rank { get; }
        public string NickName { get; }
        public UserPermissions Permissions { get; }
        public UserStatus Status => UserStatus.Unknown;
        public string StatusMessage => string.Empty;

        public MemoryMessageUser(MessageCreateEvent mce) {
            UserId = mce.UserId;
            UserName = mce.UserName;
            Colour = mce.UserColour;
            Rank = mce.UserRank;
            NickName = mce.UserNickName;
            Permissions = mce.UserPermissions;
        }

        public bool Equals(IUser other)
            => other != null && other.UserId == UserId;

        public override string ToString()
            => $@"<MemoryMessageUser {UserId}#{UserName}>";
    }
}
