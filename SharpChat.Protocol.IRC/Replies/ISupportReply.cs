using System;
using System.Text;

namespace SharpChat.Protocol.IRC.Replies {
    public class ISupportReply : Reply {
        public const int CODE = 5;

        public override int ReplyCode => CODE;

        private IRCServer Server { get; }

        public ISupportReply(IRCServer server) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        protected override string BuildLine() {
            StringBuilder sb = new StringBuilder();

            // todo: make this numbers read from configs and share them across core
            sb.Append(@"AWAYLEN=5 "); // internally this is actually 100, but leaving it at 5 until sock chat has the proper Facilities
            sb.Append(@"CASEMAPPING=ascii ");
            sb.Append(@"CHANLIMIT=#:10 "); // Limit channels to 1 (except for DMs) until Sock Chat v2
            sb.Append(@"CHANMODES=b,k,l,PTimnpstz ");
            sb.Append(@"CHANNELLEN=40 "); // Determine this
            sb.Append(@"CHANTYPES=# "); 
            sb.Append(@"FNC ");
            sb.Append(@"KEYLEN=32 "); // propagate this to /password in sokc chat
            sb.Append(@"KICKLEN=300 ");
            sb.Append(@"MAXNICKLEN=16 "); // what the fuck is the difference between this and NICKLEN, i don't get it
            sb.AppendFormat(@"NETWORK={0} ", Server.NetworkName);
            sb.Append(@"NICKLEN=16 ");
            sb.Append(@"OVERRIDE ");
            sb.Append(@"PREFIX=(qaohv)~&@%+ ");
            sb.Append(@"RFC2812 ");
            sb.Append(@"SAFELIST ");
            sb.Append(@"STD=i-d ");
            sb.Append(@"TOPICLEN=300 ");
            sb.Append(@": are supported by this server");

            return sb.ToString();
        }
    }
}
