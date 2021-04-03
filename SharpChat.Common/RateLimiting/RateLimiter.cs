using SharpChat.Configuration;
using SharpChat.Protocol;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.RateLimiting {
    public class RateLimiter {
        public const int DEFAULT_BAN_DURATION = 30;
        public const int DEFAULT_RANK_EXCEPT = 0;
        public const int DEFAULT_BACKLOG_SIZE = 30;
        public const int DEFAULT_THRESHOLD = 10;
        public const int DEFAULT_WARN_WITHIN = 5;

        private CachedValue<int> BanDurationValue { get; }
        private CachedValue<int> RankExceptionValue { get; }
        private CachedValue<int> BacklogSizeValue { get; }
        private CachedValue<int> ThresholdValue { get; }
        private CachedValue<int> WarnWithinValue { get; }

        public int BacklogSize => BacklogSizeValue;
        public int WarnWithin => WarnWithinValue;
        public TimeSpan Threshold => TimeSpan.FromSeconds(ThresholdValue);
        public TimeSpan BanDuration => TimeSpan.FromSeconds(BanDurationValue);

        private List<RateLimiterSession> Sessions { get; } = new List<RateLimiterSession>();
        private readonly object Sync = new object();

        public RateLimiter(IConfig config) {
            BanDurationValue = config.ReadCached(@"banDuration", DEFAULT_BAN_DURATION);
            RankExceptionValue = config.ReadCached(@"exceptRank", DEFAULT_RANK_EXCEPT);
            BacklogSizeValue = config.ReadCached(@"backlog", DEFAULT_BAN_DURATION);
            ThresholdValue = config.ReadCached(@"threshold", DEFAULT_THRESHOLD);
            WarnWithinValue = config.ReadCached(@"warnWithin", DEFAULT_WARN_WITHIN);
        }

        public bool HasRankException(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            int except = RankExceptionValue;
            return except > 0 && user.Rank >= except;
        }

        private RateLimiterSession GetSession(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sync) {
                RateLimiterSession sess = Sessions.FirstOrDefault(s => s.Connection == conn);
                if(sess == null)
                    Sessions.Add(sess = new RateLimiterSession(this, conn));
                return sess;
            }
        }

        public void ClearConnection(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sync)
                Sessions.RemoveAll(s => s.Connection == conn);
        }

        public RateLimitState BumpConnection(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            if(BanDurationValue == 0)
                return RateLimitState.None;
            RateLimiterSession sess;
            lock(Sync)
                sess = GetSession(conn);
            return sess.Bump();
        }
    }
}
