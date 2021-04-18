using SharpChat.Channels;
using SharpChat.Database;
using SharpChat.Users;
using System;

namespace SharpChat.Messages.Storage {
    public class ADOMessageChannel : IChannel {
        public string Name { get; }
        public string Topic => string.Empty;
        public bool IsTemporary => true;
        public int MinimumRank => 0;
        public bool AutoJoin => false;
        public uint MaxCapacity => 0;
        public long OwnerId => -1;
        public string Password => string.Empty;
        public bool HasPassword => false;

        public ADOMessageChannel(IDatabaseReader reader) {
            if(reader == null)
                throw new ArgumentNullException(nameof(reader));
            Name = reader.ReadString(@"msg_channel_name");
        }

        public bool Equals(IChannel other)
            => other != null && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);

        public override string ToString()
            => $@"<ADOMessageChannel {Name}>";
    }
}
