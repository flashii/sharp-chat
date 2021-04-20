using SharpChat.Channels;
using SharpChat.Protocol.IRC.Replies;
using SharpChat.Users;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class InviteCommand : IClientCommand { // reintroduce this into Sock Chat
        public const string NAME = @"INVITE";

        public string CommandName => NAME;
        public bool RequireSession => true;

        private UserManager Users { get; }
        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }

        public InviteCommand(UserManager users, ChannelManager channels, ChannelUserRelations channelUsers) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            string userName = ctx.Arguments.ElementAtOrDefault(0) ?? string.Empty;

            Users.GetUser(userName, user => {
                if(user == null) {
                    ctx.Connection.SendReply(new NoSuchNickReply(userName));
                    return;
                }

                string channelName = ctx.Arguments.ElementAtOrDefault(1) ?? string.Empty;
                if(string.IsNullOrWhiteSpace(channelName)) {
                    ctx.Connection.SendReply(new NoSuchChannelReply(channelName));
                    return;
                }

                Channels.GetChannelByName(channelName, channel => {
                    if(channel == null) {
                        ctx.Connection.SendReply(new NoSuchChannelReply(channelName));
                        return;
                    }

                    ChannelUsers.HasUser(channel, user, hasUser => {
                        if(!hasUser) {
                            ctx.Connection.SendReply(new UserOnChannelReply(user, channel));
                            return;
                        }

                        // todo: dispatch invite
                    });
                });
            });
        }
    }
}
