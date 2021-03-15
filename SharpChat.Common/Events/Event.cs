﻿using SharpChat.Events.Storage;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public abstract class Event : IEvent {
        public long EventId { get; }
        public abstract string Type { get; }
        public DateTimeOffset DateTime { get; }
        public IUser Sender { get; }
        public string Target { get; }

        private const long SEQ_ID_EPOCH = 1588377600000;
        private static int SequenceIdCounter = 0;

        public static long GenerateSequenceId() {
            if(SequenceIdCounter > 200)
                SequenceIdCounter = 0;
            long id = 0;
            id |= (DateTimeOffset.Now.ToUnixTimeMilliseconds() - SEQ_ID_EPOCH) << 8;
            id |= (ushort)(++SequenceIdCounter);
            return id;
        }

        protected Event(IEvent evt) {
            EventId = evt.EventId;
            DateTime = evt.DateTime;
            Sender = evt.Sender;
            Target = evt.Target;
        }
        public Event(IEventTarget target, IUser user, DateTimeOffset? dateTime = null) {
            EventId = GenerateSequenceId();
            DateTime = dateTime ?? DateTimeOffset.Now;
            Sender = user ?? throw new ArgumentNullException(nameof(user));
            Target = (target ?? throw new ArgumentNullException(nameof(target))).TargetName;
        }

        public virtual string EncodeAsJson()
            => @"{}";

        public static void RegisterConstructors(IEventStorage storage) {
            storage.RegisterConstructor(MessageCreateEvent.TYPE, MessageCreateEvent.DecodeFromJson);

            storage.RegisterConstructor(ChannelJoinEvent.TYPE, ChannelJoinEvent.DecodeFromJson);
            storage.RegisterConstructor(ChannelLeaveEvent.TYPE, ChannelLeaveEvent.DecodeFromJson);
            storage.RegisterConstructor(ChannelUpdateEvent.TYPE, ChannelUpdateEvent.DecodeFromJson);

            storage.RegisterConstructor(UserConnectEvent.TYPE, UserConnectEvent.DecodeFromJson);
            storage.RegisterConstructor(UserDisconnectEvent.TYPE, UserDisconnectEvent.DecodeFromJson);
        }
    }
}
