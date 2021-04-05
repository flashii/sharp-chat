using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Replies;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class JoinCommand : IClientCommand {
        public const string NAME = @"JOIN";

        public string CommandName => NAME;

        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }

        public JoinCommand(ChannelManager channels, ChannelUserRelations channelUsers) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            string firstArg = ctx.Arguments.ElementAtOrDefault(0) ?? string.Empty;

            if(firstArg == @"0") { // of course you would leave all channels with the JOIN command
                ChannelUsers.LeaveChannels(ctx.Session);
                return;
            }

            string[] names = firstArg.Split(',');
            string[] passwords = (ctx.Arguments.ElementAtOrDefault(1) ?? string.Empty).Split(',');

            for(int i = 0; i < names.Length; ++i) {
                string name = names[i];
                IChannel channel = Channels.GetChannel(c => name.Equals(c.GetIRCName()));

                if(channel == null) { // todo: check permissions and allow channel creation
                    ctx.Connection.SendReply(new BadChannelMaskReply(name));
                    continue;
                } else if(ChannelUsers.HasSession(channel, ctx.Session))
                    continue; // just continue if we're already in the channel

                // introduce channel bans at some point

                // introduce invites at some point

                if(channel.HasMaxCapacity() && ChannelUsers.CountUsers(channel) >= channel.MaxCapacity) {
                    ctx.Connection.SendReply(new ChannelIsFullReply(channel));
                    continue;
                }

                if(channel.HasPassword) {
                    string password = passwords.ElementAtOrDefault(i) ?? string.Empty;
                    if(!Channels.VerifyPassword(channel, password)) {
                        ctx.Connection.SendReply(new BadChannelKeyReply(channel));
                        continue;
                    }
                }

                ChannelUsers.JoinChannel(channel, ctx.Session);
            }
        }
    }
}
