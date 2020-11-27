﻿using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class UserChannelJoinEvent : ChatEvent {
        public UserChannelJoinEvent() : base() { }
        public UserChannelJoinEvent(DateTimeOffset joined, BasicUser user, IPacketTarget target) : base(joined, user, target, ChatEventFlags.Log) {}
    }
}
