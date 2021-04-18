using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class UserConnectEvent : Event {
        public const string TYPE = @"user:connect";

        public string Name { get; }
        public Colour Colour { get; }
        public int Rank { get; }
        public UserPermissions Permissions { get; }
        public string NickName { get; }
        public UserStatus Status { get; }
        public string StatusMessage { get; }

        public UserConnectEvent(IUser user)
            : base(null, user ?? throw new ArgumentNullException(nameof(user))) {
            Name = user.UserName;
            Colour = user.Colour;
            Rank = user.Rank;
            Permissions = user.Permissions;
            NickName = string.IsNullOrWhiteSpace(user.NickName) ? null : user.NickName;
            Status = user.Status;
            StatusMessage = user.StatusMessage;
        }
    }
}
