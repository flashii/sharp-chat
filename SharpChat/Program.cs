using Hamakaze;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.Database.Null;
using SharpChat.DataProvider;
using SharpChat.DataProvider.Null;
using SharpChat.Protocol.IRC;
using SharpChat.Protocol.SockChat;
using SharpChat.Reflection;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace SharpChat {
    public class Program {
        public const string CONFIG = @"sharpchat.cfg";
        public const ushort DEFAULT_PORT = 6770;

        private static string GetFlagArgument(string[] args, string flag) {
            int offset = Array.IndexOf(args, flag) + 1;
            return offset < 1 ? null : args.ElementAtOrDefault(offset);
        }

        public static void Main(string[] args) {
            Console.WriteLine(@"   _____ __                     ________          __ ");
            Console.WriteLine(@"  / ___// /_  ____ __________  / ____/ /_  ____ _/ /_");
            Console.WriteLine(@"  \__ \/ __ \/ __ `/ ___/ __ \/ /   / __ \/ __ `/ __/");
            Console.WriteLine(@" ___/ / / / / /_/ / /  / /_/ / /___/ / / / /_/ / /_  ");
            Console.WriteLine(@"/____/_/ /_/\__,_/_/  / .___/\____/_/ /_/\__,_/\__/  ");
            Console.WriteLine(@"                     / _/            Sock Chat Server");

#if DEBUG
            Console.WriteLine(@"============================================ DEBUG ==");
#endif

            string configFile = GetFlagArgument(args, @"--cfg") ?? CONFIG;

            // If the config file doesn't exist and we're using the default path, run the converter
            if(!File.Exists(configFile) && configFile == CONFIG)
                ConvertConfiguration();

            using IConfig config = new StreamConfig(configFile);

            // Load database and data provider libraries
            ReflectionUtilities.LoadAssemblies(@"SharpChat.Database.*.dll");
            ReflectionUtilities.LoadAssemblies(@"SharpChat.DataProvider.*.dll");

            // Allow forcing a sqlite database through console flags
            string sqliteDbPath = GetFlagArgument(args, @"--dbpath");
            string databaseBackendName;
            object databaseArgument;
            if(!string.IsNullOrEmpty(sqliteDbPath)) {
                Logger.Write($@"Forcing SQLite: {sqliteDbPath}");
                databaseBackendName = @"sqlite";
                databaseArgument = sqliteDbPath;
            } else {
                databaseBackendName = GetFlagArgument(args, @"--dbb") ?? config.ReadValue(@"db");
                databaseArgument = config.ScopeTo($@"db:{databaseBackendName}");
            }

            IDatabaseBackend databaseBackend = new ObjectConstructor<IDatabaseBackend, DatabaseBackendAttribute, NullDatabaseBackend>()
                .Construct(databaseBackendName, databaseArgument);

            using HttpClient httpClient = new HttpClient {
                DefaultUserAgent = @"SharpChat/1.0",
            };

            string dataProviderName = GetFlagArgument(args, @"--dpn") ?? config.ReadValue(@"dp");
            IDataProvider dataProvider = new ObjectConstructor<IDataProvider, DataProviderAttribute, NullDataProvider>()
                    .Construct(dataProviderName, config.ScopeTo($@"dp:{dataProviderName}"), httpClient);

            string portArg = GetFlagArgument(args, @"--port") ?? config.ReadValue(@"chat:port");
            if(string.IsNullOrEmpty(portArg) || !ushort.TryParse(portArg, out ushort port))
                port = DEFAULT_PORT;

            Logger.Write(@"Creating context...");
            using Context ctx = new Context(config.ScopeTo(@"chat"), databaseBackend, dataProvider);

            Logger.Write(@"Starting Sock Chat server...");
            using SockChatServer scs = new SockChatServer(ctx);
            scs.Listen(new IPEndPoint(IPAddress.Any, port));

#if DEBUG
            Logger.Write(@"Starting IRC server...");
            using IRCServer ircs = new IRCServer(ctx);
            ircs.Listen(new IPEndPoint(IPAddress.Any, 6667));
#endif

            using ManualResetEvent mre = new ManualResetEvent(false);
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
            mre.WaitOne();

            if(dataProvider is IDisposable dpd)
                dpd.Dispose();
            if(databaseBackend is IDisposable dbd)
                dbd.Dispose();
        }

        private static void ConvertConfiguration() {
            using Stream s = new FileStream(CONFIG, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            s.SetLength(0);
            s.Flush();

            using StreamWriter sw = new StreamWriter(s, new UTF8Encoding(false));

            sw.WriteLine(@"# and ; can be used at the start of a line for comments.");
            sw.WriteLine();

            sw.WriteLine(@"# General Configuration");
            sw.WriteLine($@"#chat:port               {DEFAULT_PORT}");
            sw.WriteLine($@"#chat:messages:maxLength {Messages.MessageManager.DEFAULT_LENGTH_MAX}");
            sw.WriteLine($@"#chat:sessions:timeOut   {Sessions.SessionManager.DEFAULT_TIMEOUT}");
            sw.WriteLine($@"#chat:sessions:maxCount  {Sessions.SessionManager.DEFAULT_MAX_COUNT}");
            sw.WriteLine();

            sw.WriteLine(@"# Rate Limiter Configuration, backlog > threshold > warnWithin or things will break");
            sw.WriteLine($@"#chat:flood:backlog      {RateLimiting.RateLimiter.DEFAULT_BACKLOG_SIZE}");
            sw.WriteLine($@"#chat:flood:threshold    {RateLimiting.RateLimiter.DEFAULT_THRESHOLD}");
            sw.WriteLine($@"#chat:flood:warnWithin   {RateLimiting.RateLimiter.DEFAULT_WARN_WITHIN}");
            sw.WriteLine($@"#chat:flood:banDuration  {RateLimiting.RateLimiter.DEFAULT_BAN_DURATION}");
            sw.WriteLine($@"#chat:flood:exceptRank   {RateLimiting.RateLimiter.DEFAULT_RANK_EXCEPT}");
            sw.WriteLine();

            sw.WriteLine(@"# Channels");
            sw.WriteLine(@"chat:channels lounge staff");
            sw.WriteLine();

            sw.WriteLine(@"# Lounge channel settings");
            sw.WriteLine(@"chat:channels:lounge:autoJoin true");
            sw.WriteLine();

            sw.WriteLine(@"# Staff channel settings");
            sw.WriteLine(@"chat:channels:staff:minRank 5");
            sw.WriteLine();

            const string msz_config = @"login_key.txt";

            sw.WriteLine(@"# Selected DataProvider (misuzu, null)");
            if(!File.Exists(msz_config))
                sw.WriteLine(@"dp null");
            else {
                sw.WriteLine(@"dp misuzu");
                sw.WriteLine();
                sw.WriteLine(@"# Misuzu DataProvider settings");
                sw.WriteLine(@"#db:misuzu:userId  61");
                sw.Write(@"dp:misuzu:secret   ");
                sw.Write(File.ReadAllText(msz_config).Trim());
                sw.WriteLine();
                sw.Write(@"dp:misuzu:endpoint ");
#if DEBUG
                sw.Write(@"https://misuzu.misaka.nl/_sockchat");
#else
                sw.Write(@"https://flashii.net/_sockchat");
#endif
                sw.WriteLine();
            }

            sw.WriteLine();

            const string sql_config = @"sqlite.txt";
            const string mdb_config = @"mariadb.txt";

            bool hasMDB = File.Exists(mdb_config),
                 hasSQL = File.Exists(sql_config);

            sw.WriteLine(@"# Selected Database Backend (mariadb, sqlite, null)");
            if(hasMDB)
                sw.WriteLine(@"db mariadb");
            else if(hasSQL)
                sw.WriteLine(@"db sqlite");
            else
                sw.WriteLine(@"db null");
            sw.WriteLine();

            if(hasMDB) {
                string[] mdbCfg = File.ReadAllLines(mdb_config);
                sw.WriteLine(@"# MariaDB configuration");
                sw.WriteLine($@"db:mariadb:host {mdbCfg[0]}");
                if(mdbCfg.Length > 1)
                    sw.WriteLine($@"db:mariadb:user {mdbCfg[1]}");
                else
                    sw.WriteLine($@"#db:mariadb:user <username>");
                if(mdbCfg.Length > 2)
                    sw.WriteLine($@"db:mariadb:pass {mdbCfg[2]}");
                else
                    sw.WriteLine($@"#db:mariadb:pass <password>");
                if(mdbCfg.Length > 3)
                    sw.WriteLine($@"db:mariadb:db   {mdbCfg[3]}");
                else
                    sw.WriteLine($@"#db:mariadb:db   <database>");
                sw.WriteLine();
            }

            if(hasSQL) {
                string[] sqlCfg = File.ReadAllLines(sql_config);
                sw.WriteLine(@"# SQLite configuration");
                sw.WriteLine($@"db:sqlite:path {sqlCfg[0]}");
            }

            sw.Flush();
        }
    }
}
