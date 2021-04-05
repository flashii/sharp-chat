using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class AdminEMailReply : ServerReply {
        public const int CODE = 259;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // e -mail add ress
            return @":irc@railgun.sh";
        }
    }
}
