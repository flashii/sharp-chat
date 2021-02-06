﻿using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class KickBanUserCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"kick" || name == @"ban";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            string commandName = ctx.Args.First();
            bool isBan = commandName == @"ban";

            if(!ctx.User.Can(isBan ? UserPermissions.BanUser : UserPermissions.KickUser))
                throw new CommandNotAllowedException(commandName);

            string userName = ctx.Args.ElementAtOrDefault(1);
            ChatUser user;
            if(userName == null || (user = ctx.Chat.Users.Get(userName)) == null)
                throw new UserNotFoundCommandException(userName);

            if(user == ctx.User || user.Rank >= ctx.User.Rank || ctx.Chat.Bans.Check(user) > DateTimeOffset.Now)
                throw new KickNotAllowedCommandException(user.UserName);

            bool isPermanent = isBan;
            string durationArg = ctx.Args.ElementAtOrDefault(2);
            DateTimeOffset? duration = isPermanent ? (DateTimeOffset?)DateTimeOffset.MaxValue : null;

            if(!string.IsNullOrEmpty(durationArg)) {
                if(durationArg == @"-1") {
                    duration = DateTimeOffset.MaxValue;
                    isPermanent = true;
                } else {
                    if(!double.TryParse(durationArg, out double durationRaw))
                        throw new CommandFormatException();
                    isPermanent = false;
                    duration = DateTimeOffset.Now.AddSeconds(durationRaw);
                }
            }

            ctx.Chat.BanUser(user, duration, isBan, isPermanent: isPermanent);
            return null;
        }
    }
}
