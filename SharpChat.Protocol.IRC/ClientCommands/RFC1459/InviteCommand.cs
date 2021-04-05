using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Replies;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class InviteCommand : IClientCommand { // reintroduce this into Sock Chat
        public const string NAME = @"INVITE";

        public string CommandName => NAME;

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
            IUser user = Users.GetUser(userName);

            if(user == null) {
                ctx.Connection.SendReply(new NoSuchNickReply(userName));
                return;
            }

            string channelName = ctx.Arguments.ElementAtOrDefault(1) ?? string.Empty;
            IChannel channel = string.IsNullOrWhiteSpace(channelName)
                ? null
                : Channels.GetChannel(c => channelName.Equals(c.GetIRCName()));

            if(channel == null) {
                ctx.Connection.SendReply(new NoSuchChannelReply(channelName));
                return;
            }

            if(ChannelUsers.HasUser(channel, user)) {
                ctx.Connection.SendReply(new UserOnChannelReply(user, channel));
                return;
            }

            // todo: dispatch invite
        }
    }
}
