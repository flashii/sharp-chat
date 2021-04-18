using SharpChat.Messages;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class MessageUpdateEvent : Event {
        public const string TYPE = @"message:update";

        public long MessageId { get; }
        public string Text { get; }

        public bool HasText
            => !string.IsNullOrEmpty(Text);

        public MessageUpdateEvent(IMessage message, IUser editor, string text)
            : base(editor, message.Channel) {
            MessageId = message.MessageId;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
