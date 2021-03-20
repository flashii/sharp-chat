﻿using SharpChat.Channels;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelCreateEvent : Event {
        public const string TYPE = @"channel:create";

        public override string Type => TYPE;
        public string Name { get; }
        public bool IsTemporary { get; }
        public int MinimumRank { get; }
        public string Password { get; }
        public bool AutoJoin { get; }
        public uint MaxCapacity { get; }

        public ChannelCreateEvent(IEventTarget context, IChannel channel)
            : base(context, channel.Owner) {
            Name = channel.Name;
            IsTemporary = channel.IsTemporary;
            MinimumRank = channel.MinimumRank;
            Password = channel.Password;
            AutoJoin = channel.AutoJoin;
            MaxCapacity = channel.MaxCapacity;
        }

        public override string EncodeAsJson() {
            return JsonSerializer.Serialize(new Dictionary<string, object> {
                { @"name", Name },
                { @"temp", IsTemporary },
                { @"rank", MinimumRank },
                { @"pass", Password },
                { @"auto", AutoJoin },
                { @"mcap", MaxCapacity },
            });
        }
    }
}
