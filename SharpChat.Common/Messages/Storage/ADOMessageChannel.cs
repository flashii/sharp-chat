using SharpChat.Channels;
using SharpChat.Database;
using System;

namespace SharpChat.Messages.Storage {
    public class ADOMessageChannel : IChannel {
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
        public int Order => 0;

        public ADOMessageChannel(IDatabaseReader reader) {
            if(reader == null)
                throw new ArgumentNullException(nameof(reader));
            ChannelId = reader.ReadString(@"msg_channel_id");
        }

        public bool Equals(IChannel other)
            => other != null && ChannelId.Equals(other.ChannelId);

        public override string ToString()
            => $@"<ADOMessageChannel {ChannelId}>";
    }
}
