﻿using System;

namespace SharpChat {
    public class ChatChannelTyping {
        public static TimeSpan Lifetime { get; } = TimeSpan.FromSeconds(5);

        public ChatUser User { get; }
        public DateTimeOffset Started { get; }

        public bool HasExpired
            => DateTimeOffset.Now - Started > Lifetime;

        public ChatChannelTyping(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Started = DateTimeOffset.Now;
        }
    }
}