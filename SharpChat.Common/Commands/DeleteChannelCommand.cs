﻿using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class DeleteChannelCommand : IChatCommand {
        private IUser Sender { get; }

        public DeleteChannelCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"delchan" || (name == @"delete" && args.ElementAtOrDefault(1)?.All(char.IsDigit) == false);

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            string channelName = string.Join('_', ctx.Args.Skip(1));

            if(string.IsNullOrWhiteSpace(channelName))
                throw new CommandFormatException();

            Channel channel = ctx.Chat.Channels.Get(channelName);
            if(channel == null)
                throw new ChannelNotFoundCommandException(channelName);

            if(!ctx.User.Can(UserPermissions.DeleteChannel) && channel.Owner != ctx.User)
                throw new ChannelDeletionCommandException(channel.Name);

            ctx.Chat.Channels.Remove(channel);
            ctx.User.Send(new ChannelDeleteResponsePacket(Sender, channel.Name));
            return null;
        }
    }
}
