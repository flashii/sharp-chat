using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Packets;
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
        private IUser Sender { get; }

        public JoinCommand(ChannelManager channels, ChannelUserRelations channelUsers, SessionManager sessions, IUser sender) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
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
                    ctx.Connection.SendPacket(new ChannelNotFoundErrorPacket(Sender, channelName));
                    Sessions.SwitchChannel(ctx.Session);
                    return;
                }

                ChannelUsers.HasUser(channel, ctx.User, hasUser => {
                    if(hasUser) {
                        ctx.Connection.SendPacket(new ChannelAlreadyJoinedErrorPacket(Sender, channel));
                        Sessions.SwitchChannel(ctx.Session);
                        return;
                    }

                    string password = string.Join(' ', ctx.Args.Skip(2));

                    if(!ctx.User.Can(UserPermissions.JoinAnyChannel) && channel.OwnerId != ctx.User.UserId) {
                        if(channel.MinimumRank > ctx.User.Rank) {
                            ctx.Connection.SendPacket(new ChannelInsufficientRankErrorPacket(Sender, channel));
                            Sessions.SwitchChannel(ctx.Session);
                            return;
                        }

                        // add capacity check

                        Channels.VerifyPassword(channel, password, success => {
                            if(!success) {
                                ctx.Connection.SendPacket(new ChannelInvalidPasswordErrorPacket(Sender, channel));
                                Sessions.SwitchChannel(ctx.Session);
                                return;
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
