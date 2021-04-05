using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class AdminLocation2Reply : ServerReply {
        public const int CODE = 258;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: not static?
            return @":Fundamentals for Legacy PCs";
        }
    }
}
