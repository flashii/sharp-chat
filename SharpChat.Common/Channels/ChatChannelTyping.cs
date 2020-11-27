﻿using SharpChat.Users;
using System;

namespace SharpChat.Channels {
    public class ChatChannelTyping {
        public static TimeSpan Lifetime { get; } = TimeSpan.FromSeconds(5);

        public ChatUser User { get; }
        public DateTimeOffset Started { get; }

        public bool HasExpired
            => DateTimeOffset.UtcNow - Started > Lifetime;

        public ChatChannelTyping(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Started = DateTimeOffset.UtcNow;
        }
    }
}
