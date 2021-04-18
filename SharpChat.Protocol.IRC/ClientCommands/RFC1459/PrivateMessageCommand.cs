using SharpChat.Channels;
using SharpChat.Messages;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Replies;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class PrivateMessageCommand : IClientCommand {
        public const string NAME = @"PRIVMSG";

        public string CommandName => NAME;
        public bool RequireSession => true;

        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private MessageManager Messages { get; }

        public PrivateMessageCommand(ChannelManager channels, ChannelUserRelations channelUsers, MessageManager messages) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public void HandleCommand(ClientCommandContext ctx) {
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

            Func<IChannel, bool> predicate = null;
            char channelPrefix = channelName.First();

            if(channelPrefix == '#')
                predicate = new Func<IChannel, bool>(c => channelName.Equals(c.GetIRCName()));

            if(predicate == null) {
                ctx.Connection.SendReply(new NoSuchNickReply(channelName));
                return;
            }

            Channels.GetChannel(predicate, channel => {
                if(channel == null) {
                    ctx.Connection.SendReply(new NoSuchNickReply(channelName));
                    return;
                }

                ChannelUsers.HasUser(channel, ctx.User, hasUser => {
                    if(!hasUser) {
                        ctx.Connection.SendReply(new CannotSendToChannelReply(channel));
                        return;
                    }

                    Messages.Create(ctx.Session, channel, text);
                });
            });
        }
    }
}
