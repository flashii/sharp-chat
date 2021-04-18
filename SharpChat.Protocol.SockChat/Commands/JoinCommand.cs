using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class JoinCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"join";

        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private SessionManager Sessions { get; }

        public JoinCommand(ChannelManager channels, ChannelUserRelations channelUsers, SessionManager sessions) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public bool DispatchCommand(CommandContext ctx) {
            string channelName = ctx.Args.ElementAtOrDefault(1);

            // no error, apparently
            if(string.IsNullOrWhiteSpace(channelName))
                return false;

            Channels.GetChannelByName(channelName, channel => {
                // the original server sends ForceChannel before sending the error message, but this order probably makes more sense.
                // NEW: REVERT THIS ^^^^ WHEN CONVERTING BACK TO NOT EXCEPTIONS
                // EXCEPTIONS ARE HEAVY, DON'T USE THEM FOR USER ERRORS YOU IDIOT

                if(channel == null) {
                    Sessions.SwitchChannel(ctx.Session);
                    throw new ChannelNotFoundCommandException(channelName);
                }

                ChannelUsers.HasUser(channel, ctx.User, hasUser => {
                    if(hasUser) {
                        Sessions.SwitchChannel(ctx.Session);
                        throw new AlreadyInChannelCommandException(channel);
                    }

                    string password = string.Join(' ', ctx.Args.Skip(2));

                    if(!ctx.User.Can(UserPermissions.JoinAnyChannel) && channel.OwnerId != ctx.User.UserId) {
                        if(channel.MinimumRank > ctx.User.Rank) {
                            Sessions.SwitchChannel(ctx.Session);
                            throw new ChannelRankCommandException(channel);
                        }

                        // add capacity check

                        Channels.VerifyPassword(channel, password, success => {
                            if(!success) {
                                Sessions.SwitchChannel(ctx.Session);
                                throw new ChannelPasswordCommandException(channel);
                            }

                            ChannelUsers.JoinChannel(channel, ctx.Session);
                        });
                    } else
                        ChannelUsers.JoinChannel(channel, ctx.Session);
                });
            });

            return true;
        }
    }
}
