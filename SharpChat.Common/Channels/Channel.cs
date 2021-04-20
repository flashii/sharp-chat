using SharpChat.Events;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Channels {
    public class Channel : IChannel, IEventHandler {
        public const int ID_LENGTH = 8;

        public string ChannelId { get; }
        public string Name { get; private set; }
        public string Topic { get; private set; }
        public bool IsTemporary { get; private set; }
        public int MinimumRank { get; private set; }
        public bool AutoJoin { get; private set; }
        public uint MaxCapacity { get; private set; }
        public int Order { get; private set; }
        public long OwnerId { get; private set; }

        private readonly object Sync = new object();
        private HashSet<long> Users { get; } = new HashSet<long>();
        private Dictionary<string, long> Sessions { get; } = new Dictionary<string, long>();

        public bool HasTopic
            => !string.IsNullOrWhiteSpace(Topic);

        public string Password { get; private set; } = string.Empty;
        public bool HasPassword
            => !string.IsNullOrWhiteSpace(Password);

        public Channel(
            string channelId,
            string name,
            string topic,
            bool temp,
            int minimumRank,
            string password,
            bool autoJoin,
            uint maxCapacity,
            long ownerId,
            int order
        ) {
            ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Topic = topic;
            IsTemporary = temp;
            MinimumRank = minimumRank;
            Password = password ?? string.Empty;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
            OwnerId = ownerId;
            Order = order;
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
                return Sessions.ContainsKey(session.SessionId);
        }

        public void GetUserIds(Action<IEnumerable<long>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Users);
        }

        public void GetSessionIds(Action<IEnumerable<string>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Sessions.Keys);
        }

        public int CountUsers() {
            lock(Sync)
                return Users.Count;
        }

        public int CountUserSessions(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            lock(Sync)
                return Sessions.Values.Count(u => u == user.UserId);
        }

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case ChannelUpdateEvent update: // Owner?
                    lock(Sync) {
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
                        if(update.Order.HasValue)
                            Order = update.Order.Value;
                    }
                    break;

                case ChannelUserJoinEvent cuje:
                    lock(Sync) {
                        Sessions.Add(cuje.SessionId, cuje.UserId);
                        Users.Add(cuje.UserId);
                    }
                    break;
                case ChannelSessionJoinEvent csje:
                    lock(Sync)
                        Sessions.Add(csje.SessionId, csje.UserId);
                    break;

                case ChannelUserLeaveEvent cule:
                    lock(Sync) {
                        Users.Remove(cule.UserId);
                        Queue<string> delete = new Queue<string>(Sessions.Where(s => s.Value == cule.UserId).Select(s => s.Key));
                        while(delete.TryDequeue(out string sessionId))
                            Sessions.Remove(sessionId);
                    }
                    break;
                case ChannelSessionLeaveEvent csle:
                    lock(Sync)
                        Sessions.Remove(csle.SessionId);
                    break;
            }
        }

        public bool Equals(IChannel other)
            => other != null && ChannelId.Equals(other.ChannelId);

        public override string ToString()
            => $@"<Channel {ChannelId}#{Name}>";
    }
}
