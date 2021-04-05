using SharpChat.Users;
using System;

namespace SharpChat.Channels {
    public interface IChannel : IEquatable<IChannel> {
        string Name { get; }
        string Topic { get; }
        bool IsTemporary { get; }
        int MinimumRank { get; }
        bool AutoJoin { get; }
        uint MaxCapacity { get; }
        IUser Owner { get; }

        string Password { get; }
        bool HasPassword { get; }
    }
}
