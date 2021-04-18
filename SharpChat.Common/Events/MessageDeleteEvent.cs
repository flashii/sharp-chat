using SharpChat.Messages;
using SharpChat.Users;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class MessageDeleteEvent : Event {
        public const string TYPE = @"message:delete";

        public long MessageId { get; }

        public MessageDeleteEvent(IUser actor, IMessage message)
            : base(message.Channel, actor) {
            MessageId = message.MessageId;
        }

        public MessageDeleteEvent(MessageUpdateEvent mue)
            : base(mue.ChannelName, mue.UserId) {
            MessageId = mue.MessageId;
        }
    }
}
