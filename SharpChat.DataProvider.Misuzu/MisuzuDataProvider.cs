using Hamakaze;
using SharpChat.Bans;
using SharpChat.Configuration;
using SharpChat.DataProvider.Misuzu.Bans;
using SharpChat.DataProvider.Misuzu.Users.Auth;
using SharpChat.DataProvider.Misuzu.Users.Bump;
using SharpChat.Users.Auth;
using SharpChat.Users.Bump;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat.DataProvider.Misuzu {
    [DataProvider(@"misuzu")]
    public class MisuzuDataProvider : IDataProvider {
        private HttpClient HttpClient { get; }
        private IConfig Config { get; }

        private CachedValue<string> SecretKey { get; }
        private CachedValue<string> BaseURL { get; }
        private CachedValue<long> ActorIdValue { get; }

        public IBanClient BanClient { get; }
        public IUserAuthClient UserAuthClient { get; }
        public IUserBumpClient UserBumpClient { get; }

        private const string DEFAULT_SECRET = @"woomy";

        public long ActorId => ActorIdValue;

        public MisuzuDataProvider(IConfig config, HttpClient httpClient) {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            SecretKey = Config.ReadCached(@"secret", DEFAULT_SECRET, TimeSpan.FromMinutes(1));
            BaseURL = Config.ReadCached(@"endpoint", string.Empty, TimeSpan.FromMinutes(1));
            ActorIdValue = Config.ReadCached(@"userId", 0L);

            BanClient = new MisuzuBanClient(this, HttpClient);
            UserAuthClient = new MisuzuUserAuthClient(this, HttpClient);
            UserBumpClient = new MisuzuUserBumpClient(this, HttpClient);
        }

        public string GetURL(string path)
            => BaseURL.Value + path;

        public string GetSignedHash(object obj, string key = null)
            => GetSignedHash(obj.ToString(), key);

        public string GetSignedHash(string str, string key = null)
            => GetSignedHash(Encoding.UTF8.GetBytes(str), key);

        public string GetSignedHash(byte[] bytes, string key = null) {
            StringBuilder sb = new();

            using(HMACSHA256 algo = new HMACSHA256(Encoding.UTF8.GetBytes(key ?? SecretKey))) {
                byte[] hash = algo.ComputeHash(bytes);
                foreach(byte b in hash)
                    sb.AppendFormat(@"{0:x2}", b);
            }

            return sb.ToString();
        }
    }
}
