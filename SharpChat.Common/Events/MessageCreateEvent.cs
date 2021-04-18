using SharpChat.Messages;
using SharpChat.Sessions;
using SharpChat.Users;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class MessageCreateEvent : Event {
        public const string TYPE = @"message:create";

        public long MessageId { get; }
        public string Text { get; }
        public bool IsAction { get; }

        public string UserName { get; }
        public Colour UserColour { get; }
        public int UserRank { get; }
        public string UserNickName { get; }
        public UserPermissions UserPermissions { get; }

        public MessageCreateEvent(ISession session, IMessage message)
            : base(message.Channel, session) {
            MessageId = message.MessageId;
            Text = message.Text;
            IsAction = message.IsAction;
            UserName = message.Sender.UserName;
            UserColour = message.Sender.Colour;
            UserRank = message.Sender.Rank;
            UserNickName = string.IsNullOrWhiteSpace(message.Sender.NickName) ? null : message.Sender.NickName;
            UserPermissions = message.Sender.Permissions;
        }

        // only used for a hackjob, please don't use it
        public MessageCreateEvent(string sessionId, IMessage message)
            : base(message.Sender.UserId, null, sessionId, null) {
            MessageId = message.MessageId;
            Text = message.Text;
            IsAction = message.IsAction;
            UserColour = message.Sender.Colour;
            UserRank = message.Sender.Rank;
            UserNickName = string.IsNullOrWhiteSpace(message.Sender.NickName) ? null : message.Sender.NickName;
            UserPermissions = message.Sender.Permissions;
        }
    }
}
