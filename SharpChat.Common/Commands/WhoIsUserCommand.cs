﻿using SharpChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Commands {
    public class WhoIsUserCommand : ICommand {
        private UserManager Users { get; }
        private SessionManager Sessions { get; }
        private IUser Sender { get; }

        public WhoIsUserCommand(UserManager users, SessionManager sessions, IUser sender) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"ip" || name == @"whois";

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SeeIPAddress))
                throw new CommandNotAllowedException(ctx.Args);

            string userName = ctx.Args.ElementAtOrDefault(1);
            IUser user = null;
            if(string.IsNullOrEmpty(userName) || (user = Users.GetUser(userName)) == null)
                throw new UserNotFoundCommandException(user?.UserName ?? userName);

            IEnumerable<IPAddress> addrs = Sessions.GetRemoteAddresses(user);
            foreach(IPAddress addr in addrs)
                ctx.Session.SendPacket(new WhoIsResponsePacket(Sender, user, addr));

            return true;
        }
    }
}
