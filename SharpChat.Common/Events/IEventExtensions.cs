namespace SharpChat.Events {
    public static class IEventExtensions {
        public static bool IsBroadcast(this IEvent evt)
            => evt.ChannelName == null;

        public static bool HasUser(this IEvent evt)
            => evt.UserId != null;
    }
}
