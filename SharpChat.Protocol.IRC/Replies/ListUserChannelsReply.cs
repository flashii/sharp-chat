using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class ListUserChannelsReply : Reply {
        public const int CODE = 254;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: count channels actually
            return @"60 :channels formed";
        }
    }
}
