using System.IO;
using System.Reflection;
using System.Text;

namespace SharpChat {
    public static class SharpInfo {
        private const string NAME = @"SharpChat";
        private const string UNKNOWN = @"???????";

        public static string VersionString { get; }
        public static bool IsDebugBuild { get; }

        public static string ProgramName { get; }

        static SharpInfo() {
#if DEBUG
            IsDebugBuild = true;
#endif

            try {
                using Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"SharpChat.version.txt");
                using StreamReader sr = new StreamReader(s);
                VersionString = sr.ReadLine();
            } catch {
                VersionString = UNKNOWN;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(NAME);
            sb.Append('/');

            if(IsDebugBuild)
                sb.Append(VersionString.Substring(0, 7));
            else
                sb.Append(VersionString);

            ProgramName = sb.ToString();
        }
    }
}
