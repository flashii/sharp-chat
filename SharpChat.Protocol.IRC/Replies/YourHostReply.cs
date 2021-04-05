namespace SharpChat.Protocol.IRC.Replies {
    public class YourHostReply : ServerReply {
        public const int CODE = 2;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: don't be static
            return @":Your host is irc.railgun.sh, running version SharpChat/2021xxxx";
        }
    }
}
