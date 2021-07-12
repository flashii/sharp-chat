using System;

namespace SharpChat.Configuration {
    public class CachedValue<T> {
        private IConfig Config { get; }
        private string Name { get; }
        private TimeSpan Lifetime { get; }
        private T Fallback { get; }
        private object Sync { get; } = new();

        private object CurrentValue { get; set; }
        private DateTimeOffset LastRead { get; set; }

        public T Value {
            get {
                lock(Sync) {
                    DateTimeOffset now = DateTimeOffset.Now;
                    if((now - LastRead) >= Lifetime) {
                        LastRead = now;
                        CurrentValue = Config.ReadValue(Name, Fallback);
                        Logger.Debug($@"Read {Name} ({CurrentValue})");
                    }
                }
                return (T)CurrentValue;
            }
        }

        public static implicit operator T(CachedValue<T> val) => val.Value;

        public CachedValue(IConfig config, string name, TimeSpan lifetime, T fallback) {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Lifetime = lifetime;
            Fallback = fallback;
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(@"Name cannot be empty.", nameof(name));
        }

        public void Refresh() {
            lock(Sync) {
                LastRead = DateTimeOffset.MinValue;
            }
        }
    }
}
