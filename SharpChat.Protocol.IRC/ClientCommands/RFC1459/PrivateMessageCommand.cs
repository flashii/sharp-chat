using SharpChat.Channels;
using SharpChat.Messages;
using SharpChat.Protocol.IRC.Replies;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class PrivateMessageCommand : IClientCommand {
        public const string NAME = @"PRIVMSG";

        public string CommandName => NAME;

        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private MessageManager Messages { get; }

        public PrivateMessageCommand(ChannelManager channels, ChannelUserRelations channelUsers, MessageManager messages) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            if(!ctx.HasUser)
                return;

            string channelName = ctx.Arguments.ElementAtOrDefault(0);
            if(string.IsNullOrWhiteSpace(channelName)) {
                ctx.Connection.SendReply(new NoRecipientReply(NAME));
                return;
            }

            string text = ctx.Arguments.ElementAtOrDefault(1);
            if(string.IsNullOrWhiteSpace(text)) {
                ctx.Connection.SendReply(new NoTextToSendReply());
                return;
            }

            IChannel channel = null;
            char channelPrefix = channelName.First();

            if(channelPrefix == '#')
                channel = Channels.GetChannel(channelName[1..]);

            if(channel == null) {
                ctx.Connection.SendReply(new NoSuchNickReply(channelName));
                return;
            }

            if(!ChannelUsers.HasUser(channel, ctx.User)) {
                ctx.Connection.SendReply(new CannotSendToChannelReply(channel));
                return;
            }

            Messages.Create(ctx.User, channel, text);
        }
    }
}
