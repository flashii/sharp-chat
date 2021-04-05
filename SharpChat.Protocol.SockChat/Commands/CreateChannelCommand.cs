using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class CreateChannelCommand : ICommand {
        private const string NAME = @"create";

        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private IUser Sender { get; }

        public CreateChannelCommand(ChannelManager channels, ChannelUserRelations channelUsers, IUser sender) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.CreateChannel))
                throw new CommandNotAllowedException(NAME);

            bool hasRank;
            if(ctx.Args.Count() < 2 || (hasRank = ctx.Args.ElementAtOrDefault(1)?.All(char.IsDigit) == true && ctx.Args.Count() < 3))
                throw new CommandFormatException();

            int rank = 0;
            if(hasRank && !int.TryParse(ctx.Args.ElementAtOrDefault(1), out rank) && rank < 0)
                rank = 0;

            if(rank > ctx.User.Rank)
                throw new InsufficientRankForChangeCommandException();

            string createChanName = string.Join('_', ctx.Args.Skip(hasRank ? 2 : 1));
            IChannel createChan;

            try {
                createChan = Channels.Create(
                    ctx.User,
                    createChanName,
                    null,
                    !ctx.User.Can(UserPermissions.SetChannelPermanent),
                    rank
                );
            } catch(ChannelExistException) {
                throw new ChannelExistsCommandException(createChanName);
            } catch(ChannelInvalidNameException) {
                throw new ChannelNameInvalidCommandException();
            }

            ChannelUsers.JoinChannel(createChan, ctx.Session);

            ctx.Connection.SendPacket(new ChannelCreateResponsePacket(Sender, createChan));
            return true;
        }
    }
}
