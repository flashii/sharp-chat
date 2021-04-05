using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class MyInfoReply : ServerReply {
        public const int CODE = 4;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: not static
            return @":irc.railgun.sh SharpChat/2021xxxx Oiorw hovITbceiklmnpst";
        }
    }
}
