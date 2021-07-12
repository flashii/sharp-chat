using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class KickBanUserCommand : ICommand {
        private UserManager Users { get; }
        private IUser Sender { get; }

        public KickBanUserCommand(UserManager users, IUser sender) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name is @"kick" or @"ban";

        public bool DispatchCommand(CommandContext ctx) {
            string commandName = ctx.Args.First();
            bool isBan = commandName == @"ban";

            if(!ctx.User.Can(isBan ? UserPermissions.BanUser : UserPermissions.KickUser)) {
                ctx.Connection.SendPacket(new CommandNotAllowedErrorPacket(Sender, commandName));
                return true;
            }

            string userName = ctx.Args.ElementAtOrDefault(1);
            if(string.IsNullOrEmpty(userName)) {
                ctx.Connection.SendPacket(new UserNotFoundPacket(Sender, userName));
                return true;
            }

            Users.GetUserBySockChatName(userName, user => {
                if(user == null) {
                    ctx.Connection.SendPacket(new UserNotFoundPacket(Sender, userName));
                    return;
                }

                if(user == ctx.User || user.Rank >= ctx.User.Rank) {
                    ctx.Connection.SendPacket(new KickNotAllowedErrorPacket(Sender, user.UserName));
                    return;
                }

                bool isPermanent = isBan;
                string durationArg = ctx.Args.ElementAtOrDefault(2);
                TimeSpan duration = TimeSpan.Zero;

                if(!string.IsNullOrEmpty(durationArg)) {
                    if(durationArg == @"-1") {
                        isPermanent = true;
                    } else {
                        if(!double.TryParse(durationArg, out double durationRaw)) {
                            ctx.Connection.SendPacket(new CommandFormatErrorPacket(Sender));
                            return;
                        }
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
