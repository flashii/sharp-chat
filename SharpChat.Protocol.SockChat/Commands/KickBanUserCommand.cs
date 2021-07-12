using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class KickBanUserCommand : ICommand {
        private UserManager Users { get; }

        public KickBanUserCommand(UserManager users) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name is @"kick" or @"ban";

        public bool DispatchCommand(CommandContext ctx) {
            string commandName = ctx.Args.First();
            bool isBan = commandName == @"ban";

            if(!ctx.User.Can(isBan ? UserPermissions.BanUser : UserPermissions.KickUser))
                throw new CommandNotAllowedException(commandName);

            string userName = ctx.Args.ElementAtOrDefault(1);
            if(string.IsNullOrEmpty(userName))
                throw new UserNotFoundCommandException(userName);

            Users.GetUserBySockChatName(userName, user => {
                if(user == null)
                    throw new UserNotFoundCommandException(userName);

                if(user == ctx.User || user.Rank >= ctx.User.Rank)
                    throw new KickNotAllowedCommandException(user.UserName);

                bool isPermanent = isBan;
                string durationArg = ctx.Args.ElementAtOrDefault(2);
                TimeSpan duration = TimeSpan.Zero;

                if(!string.IsNullOrEmpty(durationArg)) {
                    if(durationArg == @"-1") {
                        isPermanent = true;
                    } else {
                        if(!double.TryParse(durationArg, out double durationRaw))
                            throw new CommandFormatException();
                        isPermanent = false;
                        duration = TimeSpan.FromSeconds(durationRaw);
                    }
                }

                // TODO: allow supplying a textReason

                //ctx.Chat.BanUser(user, duration, isPermanent: isPermanent, modUser: ctx.User);
            });

            return true;
        }
    }
}
