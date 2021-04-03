﻿using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class ChannelRankCommand : ICommand {
        private ChannelManager Channels { get; }
        private IUser Sender { get; }

        public ChannelRankCommand(ChannelManager channels, IUser sender) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"rank" || name == @"hierarchy" || name == @"priv";

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SetChannelHierarchy) || ctx.Channel.Owner != ctx.User)
                throw new CommandNotAllowedException(ctx.Args);

            if(!int.TryParse(ctx.Args.ElementAtOrDefault(1), out int rank) || rank > ctx.User.Rank)
                throw new InsufficientRankForChangeCommandException();

            Channels.Update(ctx.Channel, minRank: rank);
            ctx.Connection.SendPacket(new ChannelRankResponsePacket(Sender));
            return true;
        }
    }
}