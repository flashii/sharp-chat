using System;

namespace SharpChat.Channels {
    public interface IChannel : IEquatable<IChannel> {
        string ChannelId { get; }
        string Name { get; }
        string Topic { get; }
        bool IsTemporary { get; }
        int MinimumRank { get; }
        bool AutoJoin { get; }
        uint MaxCapacity { get; }
        int Order { get; }
        long OwnerId { get; }

        string Password { get; }
        bool HasPassword { get; }
    }
}
