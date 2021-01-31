﻿using SharpChat.Users;
using System;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserConnectEvent : ChatEvent {
        public UserConnectEvent(IEvent evt, JsonElement elem) : base(evt, elem) { }
        public UserConnectEvent(DateTimeOffset joined, IUser user, IPacketTarget target) : base(joined, user, target, EventFlags.Log) {}
    }
}
