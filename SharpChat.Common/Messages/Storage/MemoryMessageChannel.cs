using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Users;
using System;

namespace SharpChat.Messages.Storage {
    public class MemoryMessageChannel : IChannel, IEventHandler {
        public string Name { get; private set; }
        public string Topic { get; private set; }
        public bool IsTemporary { get; private set; }
        public int MinimumRank { get; private set; }
        public bool AutoJoin { get; private set; }
        public uint MaxCapacity { get; private set; }
        public IUser Owner { get; private set; }
        public string Password { get; private set; }
        public bool HasPassword => !string.IsNullOrEmpty(Password);

        public MemoryMessageChannel(IEvent evt) {
            Name = evt.Channel.Name;
            Topic = evt.Channel.Topic;
            IsTemporary = evt.Channel.IsTemporary;
            MinimumRank = evt.Channel.MinimumRank;
            AutoJoin = evt.Channel.AutoJoin;
            MaxCapacity = evt.Channel.MaxCapacity;
            Owner = evt.Channel.Owner;
            Password = evt.Channel.Password;
        }

        public bool VerifyPassword(string password)
            => false;

        public bool Equals(IChannel other)
            => other != null && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case ChannelUpdateEvent cue:
                    if(cue.HasName)
                        Name = cue.Name;
                    break;
            }
        }

        public override string ToString()
            => $@"<MemoryMessageChannel {Name}>";
    }
}
