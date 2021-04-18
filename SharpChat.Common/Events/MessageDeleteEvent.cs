using SharpChat.Messages;
using SharpChat.Users;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class MessageDeleteEvent : Event {
        public const string TYPE = @"message:delete";

        public long MessageId { get; }

        public MessageDeleteEvent(IUser actor, IMessage message)
            : base(actor, message.Channel) {
            MessageId = message.MessageId;
        }

        public MessageDeleteEvent(MessageUpdateEvent mue)
            : base(mue.UserId, mue.ChannelId, null, null) {
            MessageId = mue.MessageId;
        }
    }
}
