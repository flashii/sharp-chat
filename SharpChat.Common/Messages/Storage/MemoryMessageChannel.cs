using SharpChat.Channels;
using SharpChat.Events;
using System;

namespace SharpChat.Messages.Storage {
    public class MemoryMessageChannel : IChannel, IEventHandler {
        public string Name { get; private set; }
        public string Topic => string.Empty;
        public bool IsTemporary => true;
        public int MinimumRank => 0;
        public bool AutoJoin => false;
        public uint MaxCapacity => 0;
        public long OwnerId => -1;
        public string Password => string.Empty;
        public bool HasPassword => false;

        public MemoryMessageChannel(IEvent evt) {
            Name = evt.ChannelName;
        }

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
