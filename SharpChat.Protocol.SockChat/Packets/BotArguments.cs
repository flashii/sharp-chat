using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class BotArguments {
        public const char SEPARATOR = '\f';
        
        public bool IsError { get; }
        public string Name { get; }
        public object[] Arguments { get; }

        public BotArguments(bool error, string name, params object[] args) {
            IsError = error;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Arguments = args;
        }

        public static BotArguments Notice(string name, params object[] args) {
            return new BotArguments(false, name, args);
        }

        public static BotArguments Error(string name, params object[] args) {
            return new BotArguments(true, name, args);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(IsError ? 1 : 0);
            sb.Append(SEPARATOR);
            sb.Append(Name);

            foreach(object arg in Arguments) {
                sb.Append(SEPARATOR);
                sb.Append(arg);
            }

            return sb.ToString();
        }
    }
}
