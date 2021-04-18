using SharpChat.Channels;
using SharpChat.Events;

namespace SharpChat.Messages.Storage {
    public class MemoryMessageChannel : IChannel {
        public string ChannelId { get; }
        public string Name => string.Empty;
        public string Topic => string.Empty;
        public bool IsTemporary => true;
        public int MinimumRank => 0;
        public bool AutoJoin => false;
        public uint MaxCapacity => 0;
        public long OwnerId => -1;
        public string Password => string.Empty;
        public bool HasPassword => false;

        public MemoryMessageChannel(IEvent evt) {
            ChannelId = evt.ChannelId;
        }

        public bool Equals(IChannel other)
            => other != null && ChannelId.Equals(other.ChannelId);

        public override string ToString()
            => $@"<MemoryMessageChannel {ChannelId}>";
    }
}
