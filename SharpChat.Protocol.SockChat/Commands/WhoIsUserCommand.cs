﻿using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Protocol.SockChat.Commands {
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
            => name is @"ip" or @"whois";

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SeeIPAddress)) {
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

                Sessions.GetRemoteAddresses(user, addrs => {
                    foreach(IPAddress addr in addrs)
                        ctx.Connection.SendPacket(new WhoIsResponsePacket(Sender, user, addr));
                });
            });

            return true;
        }
    }
}
