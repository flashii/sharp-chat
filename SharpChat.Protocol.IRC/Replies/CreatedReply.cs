using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class CreatedReply : Reply {
        public const int CODE = 3;

        public override int ReplyCode => CODE;

        private Context Context { get; }

        public CreatedReply(Context ctx) {
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        protected override string BuildLine() {
            return $@":This server was created {Context.Created:r}";
        }
    }
}
