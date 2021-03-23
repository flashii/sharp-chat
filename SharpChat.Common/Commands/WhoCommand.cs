﻿using SharpChat.Channels;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class WhoCommand : ICommand {
        private UserManager Users { get; }
        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private IUser Sender { get; }

        public WhoCommand(UserManager users, ChannelManager channels, ChannelUserRelations channelUsers, IUser sender) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"who";

        private void WhoServer(ICommandContext ctx) {
            Users.GetUsers(u => {
                ctx.Session.SendPacket(new UserListResponsePacket(Sender, ctx.User, u));
            });
        }

        private void WhoChannel(ICommandContext ctx, string channelName) {
            IChannel channel = Channels.GetChannel(channelName);

            if(channel == null)
                throw new ChannelNotFoundCommandException(channelName);

            if(channel.MinimumRank > ctx.User.Rank || (channel.HasPassword && !ctx.User.Can(UserPermissions.JoinAnyChannel)))
                throw new UserListChannelNotFoundCommandException(channelName);

            ChannelUsers.GetUsers(
                channel,
                users => ctx.Session.SendPacket(new UserListResponsePacket(
                    Sender,
                    channel,
                    ctx.User,
                    users.OrderByDescending(u => u.Rank)
                ))
            );
        }

        public bool DispatchCommand(ICommandContext ctx) {
            string channelName = ctx.Args.ElementAtOrDefault(1) ?? string.Empty;

            if(string.IsNullOrEmpty(channelName))
                WhoServer(ctx);
            else
                WhoChannel(ctx, channelName);

            return true;
        }
    }
}
