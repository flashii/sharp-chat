using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Protocol.SockChat.Commands;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.PacketHandlers {
    public class MessageSendPacketHandler : IPacketHandler {
        public ClientPacketId PacketId => ClientPacketId.MessageSend;

        private IEventDispatcher Dispatcher { get; }
        private MessageManager Messages { get; }
        private UserManager Users { get; }
        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private ChatBot Bot { get; }
        private IEnumerable<ICommand> Commands { get; }

        public MessageSendPacketHandler(
            UserManager users,
            ChannelManager channels,
            ChannelUserRelations channelUsers,
            MessageManager messages,
            ChatBot bot,
            IEnumerable<ICommand> commands
        ) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            Bot = bot ?? throw new ArgumentNullException(nameof(bot));
            Commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public void HandlePacket(PacketHandlerContext ctx) {
            if(ctx.Args.Count() < 3 || !ctx.HasUser || !ctx.User.Can(UserPermissions.SendMessage))
                return;

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId) || ctx.User.UserId != userId)
                return;

            // No longer concats everything after index 1 with \t, no previous implementation did that either
            string text = ctx.Args.ElementAtOrDefault(2);
            if(string.IsNullOrWhiteSpace(text))
                return;

            string channelName = ctx.Args.ElementAtOrDefault(3)?.ToLowerInvariant();
            if(string.IsNullOrWhiteSpace(channelName))
                ChannelContinue(ctx, Channels.DefaultChannel, text); // this should grab from the user, not the context wtf
            else
                Channels.GetChannel(channelName, channel => ChannelContinue(ctx, channel, text)); // this also doesn't check if we're actually in the channel
        }

        private void ChannelContinue(PacketHandlerContext ctx, IChannel channel, string text) {
            if(channel == null
            //  || !ChannelUsers.HasUser(channel, ctx.User) look below
            //  || (ctx.User.IsSilenced && !ctx.User.Can(UserPermissions.SilenceUser)) TODO: readd silencing
            )
                return;

            // this should probably check session rather than user
            ChannelUsers.HasUser(channel, ctx.User, hasUser => {
                if(hasUser)
                    return;

                if(ctx.User.Status != UserStatus.Online) {
                    Users.Update(ctx.User, status: UserStatus.Online);
                    // ChannelUsers?
                    //channel.SendPacket(new UserUpdatePacket(ctx.User));
                }

                // there's a very miniscule chance that this will return a different value on second read
                int maxLength = Messages.TextMaxLength;
                if(text.Length > maxLength)
                    text = text.Substring(0, maxLength);

                text = text.Trim();

#if DEBUG
                Logger.Write($@"<{ctx.Session.SessionId} {ctx.User.UserName}> {text}");
#endif

                bool handled = false;

                if(text[0] == '/')
                    try {
                        handled = HandleCommand(text, ctx.User, channel, ctx.Session, ctx.Connection);
                    } catch(CommandException ex) {
                        ctx.Connection.SendPacket(ex.ToPacket(Bot));
                        handled = true;
                    }

                if(!handled)
                    Messages.Create(ctx.User, channel, text);
            });
        }

        public bool HandleCommand(string message, IUser user, IChannel channel, ISession session, SockChatConnection connection) {
            string[] parts = message[1..].Split(' ');
            string commandName = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

            for(int i = 1; i < parts.Length; i++)
                parts[i] = parts[i].Replace(@"<", @"&lt;")
                                   .Replace(@">", @"&gt;")
                                   .Replace("\n", @" <br/> ");

            ICommand command = Commands.FirstOrDefault(x => x.IsCommandMatch(commandName, parts));
            if(command == null)
                throw new CommandNotFoundException(commandName);
            return command.DispatchCommand(new CommandContext(parts, user, channel, session, connection));
        }
    }
}
