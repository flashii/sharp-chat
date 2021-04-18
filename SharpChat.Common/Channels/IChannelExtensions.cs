using SharpChat.Users;

namespace SharpChat.Channels {
    public static class IChannelExtensions {
        public static bool HasMaxCapacity(this IChannel channel)
            => channel.MaxCapacity != 0;

        public static bool IsOwner(this IChannel channel, IUser user)
            => channel != null && user != null && channel.OwnerId == user.UserId;
    }
}
