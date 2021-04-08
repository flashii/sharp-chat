using System.Text;

namespace SharpChat.Protocol.IRC.Replies {
    public class ISupportReply : Reply {
        public const int CODE = 5;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            StringBuilder sb = new StringBuilder();

            // todo: make this actually mean things
            sb.Append(@"STD=draft03 ");
            sb.Append(@"PREFIX=(qaohv)~&@%+ ");
            sb.Append(@"CHANMODES=Ibe,k,l,Tcimnpst ");
            sb.Append(@"CHANTYPES=# ");
            sb.Append(@"CHANLIMIT=#:10 INEVEX=I EXCEPTS=e ");
            sb.Append(@"NETWORK=railgun ");
            sb.Append(@"CASEMAPPING=ascii ");
            sb.Append(@"CHANNELLEN=40 ");
            sb.Append(@"NICKLEN=16 ");
            sb.Append(@"MAXNICKLEN=16 ");
            sb.Append(@"TOPICLEN=300 ");
            sb.Append(@"KICKLEN=300 ");
            sb.Append(@"AWAYLEN=5 ");
            sb.Append(@"SILENCE=20 ");
            sb.Append(@"RFC2812 ");
            sb.Append(@": are supported by this server");

            return sb.ToString();
        }
    }
}
