﻿using System;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public class UserChannelJoinEvent : IChatEvent {
        [JsonIgnore]
        public DateTimeOffset DateTime { get; set; }

        [JsonIgnore]
        public BasicUser Sender { get; set; }

        [JsonIgnore]
        public IPacketTarget Target { get; set; }

        [JsonIgnore]
        public string TargetName { get; set; }

        [JsonIgnore]
        public ChatMessageFlags Flags { get; set; } = ChatMessageFlags.Log;

        [JsonIgnore]
        public long SequenceId { get; set; }

        public UserChannelJoinEvent() { }
        public UserChannelJoinEvent(DateTimeOffset joined, BasicUser user, ChatChannel target) {
            DateTime = joined;
            Sender = user;
            Target = target;
            TargetName = target?.TargetName;
        }
    }
}
