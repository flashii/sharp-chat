using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class ListUserMeReply : ServerReply {
        public const int CODE = 255;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: count local users and perhaps actually expose multi server lol
            return @":I have 500 users and 2 servers";
        }
    }
}
