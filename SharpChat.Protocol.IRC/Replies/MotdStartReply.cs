using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class MotdStartReply : ServerReply {
        public const int CODE = 375;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: unstatic address
            return @":- irc.railgun.sh Message of the day -";
        }
    }
}
