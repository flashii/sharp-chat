using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class CreatedReply : ServerReply {
        public const int CODE = 3;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: not static
            return @":This server was created 2000 years ago";
        }
    }
}
