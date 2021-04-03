namespace SharpChat.Channels {
    public static class IChannelExtensions {
        public static bool HasMaxCapacity(this IChannel channel)
            => channel.MaxCapacity != 0;
    }
}
