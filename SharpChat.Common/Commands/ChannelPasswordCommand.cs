﻿using SharpChat.Channels;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class ChannelPasswordCommand : ICommand {
        private ChannelManager Channels { get; }
        private IUser Sender { get; }

        public ChannelPasswordCommand(ChannelManager channels, IUser sender) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"password" || name == @"pwd";

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SetChannelPassword) || ctx.Channel.Owner != ctx.User)
                throw new CommandNotAllowedException(ctx.Args);

            string password = string.Join(' ', ctx.Args.Skip(1)).Trim();

            if(string.IsNullOrWhiteSpace(password))
                password = string.Empty;

            Channels.Update(ctx.Channel, password: password);
            ctx.Session.SendPacket(new ChannelPasswordResponsePacket(Sender));
            return true;
        }
    }
}
