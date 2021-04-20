using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelUpdateEvent : Event {
        public const string TYPE = @"channel:update";

        public string PreviousName { get; }
        public string Name { get; }
        public string Topic { get; }
        public bool? IsTemporary { get; }
        public int? MinimumRank { get; }
        public string Password { get; }
        public bool? AutoJoin { get; }
        public uint? MaxCapacity { get; }
        public int? Order { get; }

        public bool HasName => Name != null;
        public bool HasTopic => Topic != null;
        public bool HasPassword => Password != null;

        public ChannelUpdateEvent(
            IChannel channel,
            IUser owner,
            string name,
            string topic,
            bool? temp,
            int? minRank,
            string password,
            bool? autoJoin,
            uint? maxCapacity,
            int? order
        ) : base(owner, channel ?? throw new ArgumentNullException(nameof(channel))) {
            PreviousName = channel.Name;
            Name = name;
            Topic = topic;
            IsTemporary = temp;
            MinimumRank = minRank;
            Password = password;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
            Order = order;
        }
    }
}
