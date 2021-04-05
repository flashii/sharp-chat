using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class AdminLocation1Reply : ServerReply {
        public const int CODE = 257;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: not static?
            return @":Microsoft Windows XP";
        }
    }
}
