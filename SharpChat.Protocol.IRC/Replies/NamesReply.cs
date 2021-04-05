using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class NamesReply : ServerReply {
        public const int CODE = 353;

        public override int ReplyCode => CODE;

        public NamesReply(IChannel channel, IEnumerable<IUser> users) {
            // man i really don't
        }

        protected override string BuildLine() {
            throw new NotImplementedException();
        }
    }
}
