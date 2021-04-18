using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Replies;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class JoinCommand : IClientCommand {
        public const string NAME = @"JOIN";

        public string CommandName => NAME;
        public bool RequireSession => true;

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

                Channels.GetChannel(c => name.Equals(c.GetIRCName()), channel => {
                    if(channel == null) { // todo: check permissions and allow channel creation
                        ctx.Connection.SendReply(new BadChannelMaskReply(name));
                        return;
                    } 
                    
                    ChannelUsers.HasSession(channel, ctx.Session, hasSession => {
                        // just continue if we're already in the channel
                        if(hasSession)
                            return;

                        // introduce channel bans at some point

                        // introduce invites at some point

                        // add rank check

                        ChannelUsers.CheckOverCapacity(channel, ctx.User, isOverCapacity => {
                            if(isOverCapacity) {
                                ctx.Connection.SendReply(new ChannelIsFullReply(channel));
                                return;
                            }

                            string password = passwords.ElementAtOrDefault(i) ?? string.Empty;
                            Channels.VerifyPassword(channel, password, success => {
                                if(!success) {
                                    ctx.Connection.SendReply(new BadChannelKeyReply(channel));
                                    return;
                                }

                                ChannelUsers.JoinChannel(channel, ctx.Session);
                            });
                        });
                    });
                });
            }
        }
    }
}
