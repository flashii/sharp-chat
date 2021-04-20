using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class NickCommand : ICommand {
        private const string NAME = @"nick";
        
        private UserManager Users { get; }

        public NickCommand(UserManager users) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public bool DispatchCommand(CommandContext ctx) {
            bool setOthersNick = ctx.User.Can(UserPermissions.SetOthersNickname);

            if(!setOthersNick && !ctx.User.Can(UserPermissions.SetOwnNickname))
                throw new CommandNotAllowedException(NAME);

            if(setOthersNick && long.TryParse(ctx.Args.ElementAtOrDefault(1), out long targetUserId) && targetUserId > 0) {
                Users.GetUser(targetUserId, user => DoCommand(ctx, 2, user));
            } else
                DoCommand(ctx);

            //string previousName = targetUser == ctx.User ? (targetUser.NickName ?? targetUser.UserName) : null;
            //Users.Update(targetUser, nickName: nickStr);
            
            // both of these need to go in ChannelUsers
            //ctx.Channel.SendPacket(new UserNickChangePacket(Sender, previousName, targetUser.GetDisplayName()));
            //ctx.Channel.SendPacket(new UserUpdatePacket(targetUser));
            return true;
        }

        private void DoCommand(CommandContext ctx, int offset = 1, IUser targetUser = null) {
            targetUser ??= ctx.User;

            if(ctx.Args.Count() < offset)
                throw new CommandFormatException();

            string nickStr = string.Join('_', ctx.Args.Skip(offset)).CleanNickName().Trim();

            if(nickStr.Equals(targetUser.UserName, StringComparison.InvariantCulture))
                nickStr = null;
            else if(nickStr.Length > 15)
                nickStr = nickStr.Substring(0, 15);
            else if(string.IsNullOrEmpty(nickStr))
                nickStr = null;

            if(nickStr != null)
                Users.GetUser(nickStr, user => {
                    if(user != null)
                        throw new NickNameInUseCommandException(nickStr);
                    Users.Update(targetUser, nickName: nickStr);
                });
            else
                Users.Update(targetUser, nickName: string.Empty);
        }
    }
}
