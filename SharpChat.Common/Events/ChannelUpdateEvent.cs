﻿using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class ChannelUpdateEvent : Event {
        public const string TYPE = @"channel:update";

        public override string Type => TYPE;
        public string PreviousName { get; }
        public string Name { get; }
        public bool? IsTemporary { get; }
        public int? MinimumRank { get; }
        public string Password { get; }
        public bool? AutoJoin { get; }
        public uint? MaxCapacity { get; }

        public bool HasName => Name != null;
        public bool HasPassword => Password != null;

        public ChannelUpdateEvent(
            IChannel channel,
            IUser user,
            string name = null,
            bool? temp = null,
            int? minRank = null,
            string password = null,
            bool? autoJoin = null,
            uint? maxCapacity = null
        ) : base(channel ?? throw new ArgumentNullException(nameof(channel)), user) {
            PreviousName = channel.Name;
            Name = name;
            IsTemporary = temp;
            MinimumRank = minRank;
            Password = password;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
        }
    }
}
