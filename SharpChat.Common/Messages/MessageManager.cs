using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Messages.Storage;
using SharpChat.Sessions;
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

        public MessageManager(IEventDispatcher dispatcher, IMessageStorage storage, IConfig config) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            Config = config ?? throw new ArgumentNullException(nameof(config));

            TextMaxLengthValue = Config.ReadCached(@"maxLength", DEFAULT_LENGTH_MAX);
        }

        public Message Create(ISession session, IChannel channel, string text, bool isAction = false)
            => Create(session, session.User, channel, text, isAction);

        public Message Create(ISession session, IUser sender, IChannel channel, string text, bool isAction = false) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
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

            Message message = new(channel, sender, text, isAction);
            Dispatcher.DispatchEvent(this, new MessageCreateEvent(session, message));
            return message;
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

            MessageUpdateEvent mue = new(message, editor, text);
            if(message is IEventHandler meh)
                meh.HandleEvent(this, mue);
            Dispatcher.DispatchEvent(this, mue);
        }

        public void Delete(IUser user, IMessage message) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(message == null)
                throw new ArgumentNullException(nameof(message));

            MessageDeleteEvent mde = new(user, message);
            if(message is IEventHandler meh)
                meh.HandleEvent(this, mde);
            Dispatcher.DispatchEvent(this, mde);
        }

        public void GetMessage(long messageId, Action<IMessage> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            Storage.GetMessage(messageId, callback);
        }

        public void GetMessages(IChannel channel, Action<IEnumerable<IMessage>> callback, int amount = 20, int offset = 0) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            Storage.GetMessages(channel, callback, amount, offset);
        }

        public void HandleEvent(object sender, IEvent evt)
            => Storage.HandleEvent(sender, evt);
    }
}
