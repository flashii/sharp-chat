using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class ChannelPasswordCommand : ICommand {
        private ChannelManager Channels { get; }
        private IUser Sender { get; }

        public ChannelPasswordCommand(ChannelManager channels, IUser sender) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name is @"password" or @"pwd";

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SetChannelPassword) || ctx.Channel.OwnerId != ctx.User.UserId) {
                ctx.Connection.SendPacket(new CommandNotAllowedErrorPacket(Sender, ctx.Args));
                return true;
            }

            string password = string.Join(' ', ctx.Args.Skip(1)).Trim();

            if(string.IsNullOrWhiteSpace(password))
                password = string.Empty;

            Channels.Update(ctx.Channel, password: password);
            ctx.Connection.SendPacket(new ChannelPasswordResponsePacket(Sender));
            return true;
        }
    }
}
