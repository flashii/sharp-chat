using SharpChat.Channels;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelCreateEvent : Event {
        public const string TYPE = @"channel:create";

        public string Name { get; }
        public string Topic { get; }
        public bool IsTemporary { get; }
        public int MinimumRank { get; }
        public string Password { get; }
        public bool AutoJoin { get; }
        public uint MaxCapacity { get; }
        public int Order { get; }

        public ChannelCreateEvent(IChannel channel) : base(channel) {
            Name = channel.Name;
            Topic = channel.Topic;
            IsTemporary = channel.IsTemporary;
            MinimumRank = channel.MinimumRank;
            Password = channel.Password;
            AutoJoin = channel.AutoJoin;
            MaxCapacity = channel.MaxCapacity;
            Order = channel.Order;
        }
    }
}
