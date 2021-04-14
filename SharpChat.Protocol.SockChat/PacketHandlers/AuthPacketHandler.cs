using SharpChat.Channels;
using SharpChat.DataProvider;
using SharpChat.Messages;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpChat.Protocol.SockChat.PacketHandlers {
    public class AuthPacketHandler : IPacketHandler {
        private const string WELCOME = @"welcome.txt";

        public ClientPacketId PacketId => ClientPacketId.Authenticate;

        private SessionManager Sessions { get; }
        private UserManager Users { get; }
        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private MessageManager Messages { get; }
        private IDataProvider DataProvider { get; }
        private IUser Sender { get; }

        public AuthPacketHandler(
            SessionManager sessions,
            UserManager users,
            ChannelManager channels,
            ChannelUserRelations channelUsers,
            MessageManager messages,
            IDataProvider dataProvider,
            IUser sender
        ) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public void HandlePacket(PacketHandlerContext ctx) {
            if(ctx.HasSession)
                return;

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId) || userId < 1)
                return;

            string token = ctx.Args.ElementAtOrDefault(2);
            if(string.IsNullOrEmpty(token))
                return;

            Action<Exception> exceptionHandler = new Action<Exception>(ex => {
                Logger.Debug($@"[{ctx.Connection}] Auth fail: {ex.Message}");
                ctx.Connection.SendPacket(new AuthFailPacket(AuthFailReason.AuthInvalid));
                ctx.Connection.Close();
            });

            DataProvider.UserAuthClient.AttemptAuth(
                new UserAuthRequest(userId, token, ctx.Connection.RemoteAddress),
                res => {
                    DataProvider.BanClient.CheckBan(res.UserId, ctx.Connection.RemoteAddress, ban => {
                        if(ban.IsPermanent || ban.Expires > DateTimeOffset.Now) {
                            ctx.Connection.SendPacket(new AuthFailPacket(AuthFailReason.Banned, ban));
                            ctx.Connection.Close();
                            return;
                        }

                        Users.Connect(res, user => {
                            Sessions.HasAvailableSessions(user, available => {
                                if(!available) {
                                    ctx.Connection.SendPacket(new AuthFailPacket(AuthFailReason.MaxSessions));
                                    ctx.Connection.Close();
                                    return;
                                }

                                Sessions.Create(ctx.Connection, user, session => {
                                    ctx.Connection.SendPacket(new WelcomeMessagePacket(Sender, $@"Welcome to Flashii Chat, {user.UserName}!"));

                                    // TODO: this needs generalisation
                                    if(File.Exists(WELCOME)) {
                                        IEnumerable<string> lines = File.ReadAllLines(WELCOME).Where(x => !string.IsNullOrWhiteSpace(x));
                                        string line = lines.ElementAtOrDefault(RNG.Next(lines.Count()));

                                        if(!string.IsNullOrWhiteSpace(line))
                                            ctx.Connection.SendPacket(new WelcomeMessagePacket(Sender, line));
                                    }

                                    IChannel chan = Channels.DefaultChannel;

                                    ChannelUsers.HasUser(chan, user, hasUser => {
                                        bool shouldJoin = !hasUser;

                                        if(shouldJoin) {
                                            // ChannelUsers?
                                            //chan.SendPacket(new UserConnectPacket(DateTimeOffset.Now, user));
                                            //ctx.Chat.DispatchEvent(this, new UserConnectEvent(chan, user));
                                        }

                                        ctx.Connection.SendPacket(new AuthSuccessPacket(user, chan, session, Messages.TextMaxLength));
                                        ChannelUsers.GetUsers(chan, u => ctx.Connection.SendPacket(new ContextUsersPacket(u.Except(new[] { user }).OrderByDescending(u => u.Rank))));

                                        Messages.GetMessages(chan, m => {
                                            foreach(IMessage msg in m)
                                                ctx.Connection.SendPacket(new ContextMessagePacket(msg));
                                        });

                                        Channels.GetChannels(user.Rank, c => ctx.Connection.SendPacket(new ContextChannelsPacket(c)));

                                        if(shouldJoin)
                                            ChannelUsers.JoinChannel(chan, session);
                                    });
                                });
                            });
                        });
                    }, exceptionHandler);
                },
                exceptionHandler
            );
        }
    }
}
