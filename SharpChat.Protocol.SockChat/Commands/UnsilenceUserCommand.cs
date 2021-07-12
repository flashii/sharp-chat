using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Protocol.SockChat.Users;
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
            if(!ctx.User.Can(UserPermissions.SilenceUser)) {
                ctx.Connection.SendPacket(new CommandNotAllowedErrorPacket(Sender, ctx.Args));
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

                if(user.Rank >= ctx.User.Rank) {
                    ctx.Connection.SendPacket(new SilenceRevokeNotAllowedErrorPacket(Sender));
                    return;
                }

                //if(!user.IsSilenced) {
                //    ctx.Connection.SendPacket(new SilenceAlreadyRevokedErrorPacket(Sender));
                //    return;
                //}

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
