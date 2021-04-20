using SharpChat.Channels;
using SharpChat.DataProvider;
using SharpChat.Messages;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.Linq;

namespace SharpChat.Protocol.SockChat.PacketHandlers {
    public class AuthPacketHandler : IPacketHandler {
        public ClientPacketId PacketId => ClientPacketId.Authenticate;

        private SockChatServer Server { get; }
        private SessionManager Sessions { get; }
        private UserManager Users { get; }
        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private MessageManager Messages { get; }
        private IDataProvider DataProvider { get; }
        private IUser Sender { get; }
        private WelcomeMessage WelcomeMessage { get; }

        public AuthPacketHandler(
            SockChatServer server,
            SessionManager sessions,
            UserManager users,
            ChannelManager channels,
            ChannelUserRelations channelUsers,
            MessageManager messages,
            IDataProvider dataProvider,
            IUser sender,
            WelcomeMessage welcomeMessage
        ) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            WelcomeMessage = welcomeMessage ?? throw new ArgumentNullException(nameof(welcomeMessage));
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
                                    string welcome = Server.WelcomeMessage;
                                    if(!string.IsNullOrWhiteSpace(welcome))
                                        ctx.Connection.SendPacket(new WelcomeMessagePacket(Sender, welcome.Replace(@"{username}", user.UserName)));

                                    if(WelcomeMessage.HasRandom) {
                                        string line = WelcomeMessage.GetRandomString();
                                        if(!string.IsNullOrWhiteSpace(line))
                                            ctx.Connection.SendPacket(new WelcomeMessagePacket(Sender, line));
                                    }

                                    Channels.GetDefaultChannels(channels => {
                                        if(!channels.Any())
                                            return; // what do, this is impossible

                                        // other channels should be joined if MCHAN has been received
                                        IChannel firstChan = channels.FirstOrDefault();

                                        ctx.Connection.LastChannel = firstChan;
                                        ctx.Connection.SendPacket(new AuthSuccessPacket(user, firstChan, session, Messages.TextMaxLength));

                                        Channels.GetChannels(user.Rank, c => ctx.Connection.SendPacket(new ContextChannelsPacket(c)));
                                        ChannelUsers.JoinChannel(firstChan, ctx.Session);
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
