﻿using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Messages.Storage;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Messages {
    public class MessageManager : IEventHandler {
        private IEventDispatcher Dispatcher { get; }
        private IMessageStorage Storage { get; }
        private IConfig Config { get; }

        public const int DEFAULT_LENGTH_MAX = 2100;
        private CachedValue<int> TextMaxLengthValue { get; }
        public int TextMaxLength => TextMaxLengthValue;

        private readonly object Sync = new object();

        public MessageManager(IEventDispatcher dispatcher, IMessageStorage storage, IConfig config) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            Config = config ?? throw new ArgumentNullException(nameof(config));

            TextMaxLengthValue = Config.ReadCached(@"maxLength", DEFAULT_LENGTH_MAX);
        }

        public Message Create(IUser sender, IChannel channel, string text, bool isAction = false) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(text == null)
                throw new ArgumentNullException(nameof(text));

            if(string.IsNullOrWhiteSpace(text))
                throw new ArgumentException(@"Provided text is empty.", nameof(text));
            if(text.Length > TextMaxLength)
                throw new ArgumentException(@"Provided text is too long.", nameof(text));

            lock(Sync) {
                Message message = new Message(channel, sender, text, isAction);
                Dispatcher.DispatchEvent(this, new MessageCreateEvent(message));
                return message;
            }
        }

        public void Edit(IUser editor, IMessage message, string text = null) {
            if(editor == null)
                throw new ArgumentNullException(nameof(editor));
            if(message == null)
                throw new ArgumentNullException(nameof(message));

            if(text == null)
                return;
            if(string.IsNullOrWhiteSpace(text))
                throw new ArgumentException(@"Provided text is empty.", nameof(text));
            if(text.Length > TextMaxLength)
                throw new ArgumentException(@"Provided text is too long.", nameof(text));

            lock(Sync) {
                MessageUpdateEvent mue = new MessageUpdateEvent(message, editor, text);
                if(message is IEventHandler meh)
                    meh.HandleEvent(this, mue);
                Dispatcher.DispatchEvent(this, mue);
            }
        }

        public void Delete(IUser user, IMessage message) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(message == null)
                throw new ArgumentNullException(nameof(message));

            lock(Sync) {
                MessageDeleteEvent mde = new MessageDeleteEvent(message.Channel, user, message.MessageId);
                if(message is IEventHandler meh)
                    meh.HandleEvent(this, mde);
                Dispatcher.DispatchEvent(this, mde);
            }
        }

        public IMessage GetMessage(long messageId)
            => Storage.GetMessage(messageId);

        public IEnumerable<IMessage> GetMessages(IChannel channel, int amount, int offset)
            => Storage.GetMessages(channel, amount, offset);

        public void HandleEvent(object sender, IEvent evt)
            => Storage.HandleEvent(sender, evt);
    }
}
