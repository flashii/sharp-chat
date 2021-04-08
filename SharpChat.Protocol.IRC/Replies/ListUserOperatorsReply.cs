using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class ListUserOperatorsReply : Reply {
        public const int CODE = 252;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: count users with a high rank or something
            return @"1 :operator(s) online";
        }
    }
}
