namespace SharpChat.Events {
    public static class IEventExtensions {
        public static bool IsBroadcast(this IEvent evt)
            => evt.ChannelId == null;
    }
}
