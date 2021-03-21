﻿using SharpChat.Messages;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class MessageUpdateEvent : Event {
        public const string TYPE = @"message:update";

        public override string Type => TYPE;
        public long MessageId { get; }
        public string Text { get; }

        public bool HasText
            => !string.IsNullOrEmpty(Text);

        public MessageUpdateEvent(IMessage message, IUser editor, string text)
            : base(message.Channel, editor) {
            MessageId = message.MessageId;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
