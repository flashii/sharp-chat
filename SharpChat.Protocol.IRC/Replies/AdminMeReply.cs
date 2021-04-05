using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class AdminMeReply : ServerReply {
        public const int CODE = 256;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo make server name not static
            return @"irc.railgun.sh :Administrative info";
        }
    }
}
