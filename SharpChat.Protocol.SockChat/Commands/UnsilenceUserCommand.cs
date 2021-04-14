using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class UnsilenceUserCommand : ICommand {
        private UserManager Users { get; }
        private IUser Sender { get; }

        public UnsilenceUserCommand(UserManager users, IUser sender) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"unsilence";

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SilenceUser))
                throw new CommandNotAllowedException(ctx.Args);

            string userName = ctx.Args.ElementAtOrDefault(1);
            if(string.IsNullOrEmpty(userName))
                throw new UserNotFoundCommandException(userName);

            Users.GetUser(userName, user => {
                if(user == null)
                    throw new UserNotFoundCommandException(userName);
                if(user.Rank >= ctx.User.Rank)
                    throw new RevokeSilenceNotAllowedCommandException();

                //if(!user.IsSilenced)
                //    throw new NotSilencedCommandException();

                //ctx.Chat.Users.RevokeSilence(user);

                // UserManager
                //user.SendPacket(new SilenceRevokeNoticePacket(Sender));

                // Remain? Also UserManager?
                ctx.Connection.SendPacket(new SilenceRevokeResponsePacket(Sender, user));
            });
            return true;
        }
    }
}
