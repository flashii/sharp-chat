using SharpChat.Protocol.IRC.Replies;
using SharpChat.Users;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class NickCommand : IClientCommand {
        public const string NAME = @"NICK";

        public string CommandName => NAME;
        public bool RequireSession => false;

        private UserManager Users { get; }

        public NickCommand(UserManager users) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            if(ctx.User == null) // blocking calls to this without an error
                return;

            // TODO: check if user is allowed to set a nick
            //       should prefixes be a thing for IRC?
            //       should the prefix be nuked in favour of a forced name change?

            string nickName = ctx.Arguments.FirstOrDefault();

            if(string.IsNullOrWhiteSpace(nickName)) {
                ctx.Connection.SendReply(new NoNickNameGivenReply());
                return;
            }

            nickName = nickName.Trim();

            if(nickName.Equals(ctx.User.UserName, StringComparison.InvariantCulture)) // allowing capitalisation changes
                nickName = null;
            else if(nickName.Length > 15) // should be configurable somewhere, also magic number in Sock Chat's impl
                nickName = nickName.Substring(0, 15); // also Flashii's max username length is 16, guessing it was 15 to account for the ~?
            else if(string.IsNullOrEmpty(nickName))
                nickName = null;

            if(nickName == null) {
                Users.Update(ctx.User, nickName: string.Empty);
                return;
            }

            // TODO: global name validation routines
            //ctx.Connection.SendReply(new ErroneousNickNameReply(nickName));

            Users.GetUser(nickName, user => {
                if(user != null) {
                    ctx.Connection.SendReply(new NickNameInUseReply(nickName));
                    return;
                }

                Users.Update(ctx.User, nickName: nickName);
            });
        }
    }
}
