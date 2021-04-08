using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class ListUserUnknownReply : Reply {
        public const int CODE = 253;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: count unknown connections? whatever the fuck that means
            return @"0 :unknown connection(s)";
        }
    }
}
