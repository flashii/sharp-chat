using SharpChat.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.RateLimiting {
    public class RateLimiterSession {
        public IConnection Connection { get; }

        private Queue<DateTimeOffset> TimePoints { get; } = new();
        private readonly object Sync = new();

        private RateLimiter Limiter { get; }

        public RateLimiterSession(RateLimiter limiter, IConnection conn) {
            Limiter = limiter ?? throw new ArgumentNullException(nameof(limiter));
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        public RateLimitState Bump() {
            lock(Sync) {
                int backlogSize = Limiter.BacklogSize;

                while(TimePoints.Count >= backlogSize)
                    TimePoints.Dequeue();
                TimePoints.Enqueue(DateTimeOffset.Now);

                if(TimePoints.Count >= backlogSize) {
                    TimeSpan threshold = Limiter.Threshold;

                    if((TimePoints.Last() - TimePoints.First()) <= threshold)
                        return RateLimitState.Drop;
                    if((TimePoints.Last() - TimePoints.Skip(Limiter.WarnWithin).First()) <= threshold)
                        return RateLimitState.Warn;
                }

                return RateLimitState.None;
            }
        }
    }
}
