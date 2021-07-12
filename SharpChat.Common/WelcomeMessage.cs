using SharpChat.Configuration;
using System;
using System.IO;
using System.Linq;

namespace SharpChat {
    public class WelcomeMessage {
        private CachedValue<string> RandomFile { get; }

        private TimeSpan CacheLife { get; } = TimeSpan.FromMinutes(10);
        private bool HasRandomValue { get; set; }
        private DateTimeOffset LastRandomRead { get; set; } = DateTimeOffset.MinValue;

        private readonly object Sync = new();

        public bool HasRandom {
            get {
                lock(Sync) {
                    if(DateTimeOffset.Now - LastRandomRead >= CacheLife)
                        ReloadRandomFile();

                    return HasRandomValue;
                }
            }
        }

        private string[] RandomStrings { get; set; }

        public WelcomeMessage(IConfig config) {
            if(config == null)
                throw new ArgumentNullException(nameof(config));

            RandomFile = config.ReadCached(@"random", string.Empty, CacheLife);
        }

        public void ReloadRandomFile() {
            lock(Sync) {
                string randomFile = RandomFile;

                if(File.Exists(randomFile)) {
                    RandomStrings = File.ReadAllLines(randomFile).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                    HasRandomValue = RandomStrings.Length > 0;
                } else {
                    HasRandomValue = false;
                    RandomStrings = null;
                }

                HasRandomValue = RandomStrings?.Length > 0;
                LastRandomRead = DateTimeOffset.Now;
            }
        }

        public string GetRandomString() {
            lock(Sync)
                return HasRandom
                    ? RandomStrings.ElementAtOrDefault(RNG.Next(RandomStrings.Length))
                    : string.Empty;
        }
    }
}
