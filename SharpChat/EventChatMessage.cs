﻿using System;
using System.Text;

namespace SharpChat
{
    public class EventChatMessage : IChatMessage
    {
        public int MessageId { get; set; }
        public string MessageIdStr { get; set; }
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;
        public SockChatMessageFlags Flags { get; set; }
        public SockChatChannel Channel { get; set; }
        public SockChatUser User => SockChatServer.Bot;

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendNum(IsError);
                sb.Append(Constants.MISC_SEPARATOR);
                sb.Append(EventName);

                foreach (object part in Parts)
                {
                    sb.Append(Constants.MISC_SEPARATOR);
                    sb.Append(part);
                }

                return sb.ToString();
            }
        }

        public bool IsError { get; set; }
        public string EventName { get; set; }
        public object[] Parts { get; set; }

        public EventChatMessage(SockChatChannel chan, int msgId, SockChatMessageFlags flags, bool error, string eventName, params object[] parts)
        {
            Channel = chan;
            MessageId = msgId;
            Flags = flags;
            IsError = error;
            EventName = eventName;
            Parts = parts;
        }
        public EventChatMessage(SockChatChannel chan, string msgId, SockChatMessageFlags flags, bool error, string eventName, params object[] parts)
        {
            Channel = chan;
            MessageIdStr = msgId;
            Flags = flags;
            IsError = error;
            EventName = eventName;
            Parts = parts;
        }

        // this is cursed

        public static EventChatMessage Info(int msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, SockChatMessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(int msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(SockChatChannel chan, int msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, SockChatMessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(SockChatChannel chan, int msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(string msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, SockChatMessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(string msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(SockChatChannel chan, string msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, SockChatMessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(SockChatChannel chan, string msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, false, eventName, parts);

        public static EventChatMessage Error(int msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, SockChatMessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(int msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(SockChatChannel chan, int msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, SockChatMessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(SockChatChannel chan, int msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(string msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, SockChatMessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(string msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(SockChatChannel chan, string msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, SockChatMessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(SockChatChannel chan, string msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, true, eventName, parts);
    }
}
