﻿using System;

namespace SharpChat
{
    public class ChatMessage : IChatMessage
    {
        public int MessageId { get; set; }
        public ChatUser User { get; set; }
        public ChatChannel Channel { get; set; }
        public string Text { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public SockChatMessageFlags Flags { get; set; } = SockChatMessageFlags.RegularUser;

        public static string PackBotMessage(int type, string id, params string[] parts)
        {
            return type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);
        }
    }
}
