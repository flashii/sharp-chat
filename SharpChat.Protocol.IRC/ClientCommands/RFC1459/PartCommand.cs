using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Replies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class PartCommand : IClientCommand {
        public const string NAME = @"PART";

        public string CommandName => NAME;

        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }

        public PartCommand(ChannelManager channels, ChannelUserRelations channelUsers) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            IEnumerable<string> channelNames = (ctx.Arguments.FirstOrDefault() ?? string.Empty).Split(',').Select(n => n.ToLowerInvariant());

            if(!channelNames.Any()) {
                ctx.Connection.SendReply(new NeedMoreParamsReply(NAME));
                return;
            }

            foreach(string channelName in channelNames) {
                Channels.GetChannel(c => channelName.Equals(c.GetIRCName()), channel => {
                    if(channel == null) {
                        ctx.Connection.SendReply(new NoSuchChannelReply(channelName));
                        return;
                    }

                    ChannelUsers.HasSession(channel, ctx.Session, hasUser => {
                        if(!hasUser) {
                            ctx.Connection.SendReply(new NotOnChannelReply(channel));
                            return;
                        }

                        ChannelUsers.LeaveChannel(channel, ctx.Session);
                    });
                });
            }
        }
    }
}
