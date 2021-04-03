using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class DeleteChannelCommand : ICommand {
        private ChannelManager Channels { get; }
        private IUser Sender { get; }

        public DeleteChannelCommand(ChannelManager channels, IUser sender) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"delchan" || (name == @"delete" && args.ElementAtOrDefault(1)?.All(char.IsDigit) == false);

        public bool DispatchCommand(CommandContext ctx) {
            string channelName = string.Join('_', ctx.Args.Skip(1));

            if(string.IsNullOrWhiteSpace(channelName))
                throw new CommandFormatException();

            IChannel channel = Channels.GetChannel(channelName);
            if(channel == null)
                throw new ChannelNotFoundCommandException(channelName);

            if(!ctx.User.Can(UserPermissions.DeleteChannel) && channel.Owner != ctx.User)
                throw new ChannelDeletionCommandException(channel.Name);

            Channels.Remove(channel);
            ctx.Connection.SendPacket(new ChannelDeleteResponsePacket(Sender, channel.Name));
            return true;
        }
    }
}
