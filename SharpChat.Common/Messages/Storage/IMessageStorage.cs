using SharpChat.Channels;
using SharpChat.Events;
using System;
using System.Collections.Generic;

namespace SharpChat.Messages.Storage {
    public interface IMessageStorage : IEventHandler {
        void GetMessage(long messageId, Action<IMessage> callback);
        void GetMessages(IChannel channel, Action<IEnumerable<IMessage>> callback, int amount, int offset);
    }
}
