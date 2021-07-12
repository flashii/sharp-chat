using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class SilenceUserCommand : ICommand {
        private UserManager Users { get; }
        private IUser Sender { get; }

        public SilenceUserCommand(UserManager users, IUser sender) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"silence";

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

                if(user == ctx.User) {
                    ctx.Connection.SendPacket(new SilenceSelfErrorPacket(Sender));
                    return;
                }

                if(user.Rank >= user.Rank) {
                    ctx.Connection.SendPacket(new SilenceNotAllowedErrorPacket(Sender));
                    return;
                }

                //if(user.IsSilenced) {
                //    ctx.Connection.SendPacket(new SilencedAlreadyErrorPacket(Sender));
                //    return;
                //}

                string durationArg = ctx.Args.ElementAtOrDefault(2);

                if(!string.IsNullOrEmpty(durationArg)) {
                    if(!double.TryParse(durationArg, out double durationRaw)) {
                        ctx.Connection.SendPacket(new CommandFormatErrorPacket(Sender));
                        return;
                    }
                    //ctx.Chat.Users.Silence(user, TimeSpan.FromSeconds(durationRaw));
                } //else
                  //ctx.Chat.Users.Silence(user);

                // UserManager
                //user.SendPacket(new SilenceNoticePacket(Sender));

                // Remain? Also UserManager?
                ctx.Connection.SendPacket(new SilenceResponsePacket(Sender, user));
            });
            return true;
        }
    }
}
