namespace SharpChat.Protocol.SockChat {
    public static class SockChatExtensions {
        public static string CleanTextForMessage(this string str) {
            return str
                .Replace("\t", @"    ")
                .Replace(@"<", @"&lt;")
                .Replace(@">", @"&gt;")
                .Replace("\n", @" <br/> ");
        }

        public static string CleanCommandName(this string str) {
            return str.Replace(@".", string.Empty).ToLowerInvariant();
        }

        public static string CleanTextForCommand(this string str) {
            return str
                .Replace(@"<", @"&lt;")
                .Replace(@">", @"&gt;")
                .Replace("\n", @" <br/> ");
        }

        public static string CleanNickName(this string nick) {
            return nick
                .Replace(' ', '_')
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\f", string.Empty)
                .Replace("\t", string.Empty);
        }
    }
}
