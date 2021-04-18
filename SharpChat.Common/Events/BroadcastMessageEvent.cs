using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class BroadcastMessageEvent : Event {
        public const string TYPE = @"broadcast:message";

        public string Text { get; }

        public BroadcastMessageEvent(ChatBot chatBot, string text) : base(chatBot) {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
