using Hamakaze;
using SharpChat.Bans;
using SharpChat.Configuration;
using SharpChat.DataProvider;
using SharpChat.DataProvider.Misuzu;
using SharpChat.Users;
using SharpChat.Users.Auth;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using static System.Console;

namespace MisuzuDataProviderTest {
    public static class Program {
        public static void Main() {
            WriteLine("Misuzu Authentication Tester");

            using ManualResetEvent mre = new(false);

            string cfgPath = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            string buildMode = Path.GetFileName(cfgPath);
            cfgPath = Path.Combine(
                Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(cfgPath))),
                @"SharpChat", @"bin", buildMode, @"net5.0", @"sharpchat.cfg"
            );

            WriteLine($@"Reading config from {cfgPath}");

            using IConfig config = new StreamConfig(cfgPath);

            WriteLine($@"Enter token found on {config.ReadValue(@"dp:misuzu:endpoint")}/login:");
            string[] token = ReadLine().Split(new[] { '_' }, 2);

            HttpClient.Instance.DefaultUserAgent = @"SharpChat/1.0";

            IDataProvider dataProvider = new MisuzuDataProvider(config.ScopeTo(@"dp:misuzu"), HttpClient.Instance);

            long userId = long.Parse(token[0]);
            IPAddress remoteAddr = IPAddress.Parse(@"1.2.4.8");

            IUserAuthResponse authRes = null;
            mre.Reset();
            dataProvider.UserAuthClient.AttemptAuth(
                new UserAuthRequest(userId, token[1], remoteAddr),
                onSuccess: res => {
                    authRes = res;
                    WriteLine(@"Auth success!");
                    WriteLine($@" User ID:   {authRes.UserId}");
                    WriteLine($@" Username:  {authRes.UserName}");
                    WriteLine($@" Colour:    {authRes.Colour.Raw:X8}");
                    WriteLine($@" Hierarchy: {authRes.Rank}");
                    WriteLine($@" Silenced:  {authRes.SilencedUntil}");
                    WriteLine($@" Perms:     {authRes.Permissions}");
                    mre.Set();
                },
                onFailure: ex => {
                    WriteLine($@"Auth failed: {ex.Message}");
                    mre.Set();
                }
            );
            mre.WaitOne();

            if(authRes == null)
                return;

#if FUCKED
            WriteLine(@"Bumping last seen...");
            mre.Reset();
            dataProvider.UserBumpClient.SubmitBumpUsers(
                new[] { new User(authRes) },
                onSuccess: () => mre.Set(),
                onFailure: ex => {
                    WriteLine($@"Bump failed: {ex.Message}");
                    mre.Set();
                }
            );
            mre.WaitOne();
#endif

            WriteLine(@"Fetching ban list...");
            IEnumerable<IBanRecord> bans = Enumerable.Empty<IBanRecord>();

            mre.Reset();
            dataProvider.BanClient.GetBanList(x => { bans = x; mre.Set(); }, e => { WriteLine(e); mre.Set(); });
            mre.WaitOne();

            WriteLine($@"{bans.Count()} BANS");
            foreach(IBanRecord ban in bans) {
                WriteLine($@"BAN INFO");
                WriteLine($@" User ID:    {ban.UserId}");
                WriteLine($@" Username:   {ban.Username}");
                WriteLine($@" IP Address: {ban.UserIP}");
                WriteLine($@" Expires:    {ban.Expires}");
            }
        }
    }
}
