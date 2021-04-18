using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class UserDisconnectEvent : Event {
        public const string TYPE = @"user:disconnect";

        public UserDisconnectReason Reason { get; }

        public UserDisconnectEvent(IUser user, UserDisconnectReason reason)
            : base(user ?? throw new ArgumentNullException(nameof(user))) {
            Reason = reason;
        }
    }
}
