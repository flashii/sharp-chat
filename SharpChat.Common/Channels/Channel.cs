using SharpChat.Events;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Channels {
    public class Channel : IChannel, IEventHandler {
        public string Name { get; private set; }
        public string Topic { get; private set; }
        public bool IsTemporary { get; private set; }
        public int MinimumRank { get; private set; }
        public bool AutoJoin { get; private set; }
        public uint MaxCapacity { get; private set; }
        public IUser Owner { get; private set; }

        private readonly object Sync = new object();
        private HashSet<long> Users { get; } = new HashSet<long>();
        private HashSet<(string sessionId, long userId)> Sessions { get; } = new HashSet<(string, long)>();

        public bool HasTopic
            => !string.IsNullOrWhiteSpace(Topic);

        public string Password { get; private set; } = string.Empty;
        public bool HasPassword
            => !string.IsNullOrWhiteSpace(Password);

        public Channel(
            string name,
            string topic = null,
            bool temp = false,
            int minimumRank = 0,
            string password = null,
            bool autoJoin = false,
            uint maxCapacity = 0,
            IUser owner = null
        ) {
            Name = name;
            Topic = topic;
            IsTemporary = temp;
            MinimumRank = minimumRank;
            Password = password ?? string.Empty;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
            Owner = owner;
        }

        public bool VerifyPassword(string password) {
            if(password == null)
                throw new ArgumentNullException(nameof(password));
            lock(Sync)
                return !HasPassword || Password.Equals(password);
        }

        public bool HasUser(IUser user) {
            if(user == null)
                return false;
            lock(Sync)
                return Users.Contains(user.UserId);
        }

        public bool HasSession(ISession session) {
            if(session == null)
                return false;
            lock(Sync)
                return Sessions.Any(su => su.sessionId.Equals(session.SessionId));
        }

        public void GetUserIds(Action<IEnumerable<long>> callable) {
            if(callable == null)
                throw new ArgumentNullException(nameof(callable));
            lock(Sync)
                callable(Users);
        }

        public int CountUsers() {
            lock(Sync)
                return Users.Count;
        }

        public int CountUserSessions(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            lock(Sync)
                return Sessions.Count(s => s.userId == user.UserId);
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync)
                switch(evt) {
                    case ChannelUpdateEvent update: // Owner?
                        if(update.HasName)
                            Name = update.Name;
                        if(update.HasTopic)
                            Topic = update.Topic;
                        if(update.IsTemporary.HasValue)
                            IsTemporary = update.IsTemporary.Value;
                        if(update.MinimumRank.HasValue)
                            MinimumRank = update.MinimumRank.Value;
                        if(update.HasPassword)
                            Password = update.Password;
                        if(update.AutoJoin.HasValue)
                            AutoJoin = update.AutoJoin.Value;
                        if(update.MaxCapacity.HasValue)
                            MaxCapacity = update.MaxCapacity.Value;
                        break;

                    case ChannelUserJoinEvent cuje:
                        Sessions.Add((cuje.SessionId, cuje.User.UserId));
                        Users.Add(cuje.User.UserId);
                        break;
                    case ChannelSessionJoinEvent csje:
                        Sessions.Add((csje.SessionId, csje.User.UserId));
                        break;

                    case ChannelUserLeaveEvent cule:
                        Users.Remove(cule.User.UserId);
                        Sessions.RemoveWhere(su => su.userId == cule.User.UserId);
                        break;
                    case ChannelSessionLeaveEvent csle:
                        Sessions.RemoveWhere(su => su.sessionId.Equals(csle.SessionId));
                        break;
                }
        }

        public bool Equals(IChannel other)
            => other != null && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);

        public override string ToString()
            => $@"<Channel {Name}>";
    }
}
