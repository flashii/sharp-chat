using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class ListUserClientReply : Reply {
        public const int CODE = 251;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: make this real
            return @":There are 810 users and 25 services on 3510 servers";
        }
    }
}
