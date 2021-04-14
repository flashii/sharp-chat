using SharpChat.Channels;
using SharpChat.Protocol.IRC.Replies;
using System;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class ListCommand : IClientCommand {
        public const string NAME = @"LIST";

        public string CommandName => NAME;

        public ChannelManager Channels { get; }
        public ChannelUserRelations ChannelUsers { get; }

        public ListCommand(ChannelManager channels, ChannelUserRelations channelUsers) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            // todo: special LIST comments for Modern IRC

            Channels.GetChannels(channels => { // probably needs to check if a user actually has access
                foreach(IChannel channel in channels)
                    ChannelUsers.CountUsers(channel, userCount => ctx.Connection.SendReply(new ListItemReply(channel, userCount)));

                ctx.Connection.SendReply(new ListEndReply());
            });
        }
    }
}
