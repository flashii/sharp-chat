using SharpChat.Channels;
using SharpChat.DataProvider;
using SharpChat.Protocol.IRC.Replies;
using SharpChat.Protocol.IRC.ServerCommands;
using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class UserCommand : IClientCommand {
        public const string NAME = @"USER";

        public string CommandName => NAME;
        public bool RequireSession => false;

        private IRCServer Server { get; }
        private Context Context { get; }
        private UserManager Users { get; }
        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private SessionManager Sessions { get; }
        private IDataProvider DataProvider { get; }
        private WelcomeMessage WelcomeMessage { get; }

        public UserCommand(
            IRCServer server,
            Context context,
            UserManager users,
            ChannelManager channels,
            ChannelUserRelations channelUsers,
            SessionManager sessions,
            IDataProvider dataProvider,
            WelcomeMessage welcomeMessage
        ) {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            WelcomeMessage = welcomeMessage ?? throw new ArgumentNullException(nameof(welcomeMessage));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            if(ctx.Connection.HasAuthenticated) {
                ctx.Connection.SendReply(new AlreadyRegisteredReply());
                return;
            }

            // just drop subsequent calls
            if(ctx.Connection.IsAuthenticating)
                return;
            ctx.Connection.IsAuthenticating = true;

            // should accept normal text username in the future
            if(!long.TryParse(ctx.Arguments.FirstOrDefault(), out long userId)) {
                ctx.Connection.SendReply(new PasswordMismatchReply());
                ctx.Connection.Close();
                return;
            }

            Action<Exception> exceptionHandler = new Action<Exception>(ex => {
                Logger.Debug($@"[{ctx.Connection}] Auth fail: {ex.Message}");
                ctx.Connection.SendReply(new PasswordMismatchReply());
                ctx.Connection.Close();
            });

            DataProvider.UserAuthClient.AttemptAuth(
                new UserAuthRequest(userId, ctx.Connection.Password, ctx.Connection.RemoteAddress),
                res => {
                    ctx.Connection.Password = null;
                    ctx.Connection.HasAuthenticated = true;

                    DataProvider.BanClient.CheckBan(res.UserId, ctx.Connection.RemoteAddress, ban => {
                        if(ban.IsPermanent || ban.Expires > DateTimeOffset.Now) {
                            // should probably include the time

                            ctx.Connection.SendReply(new YouAreBannedReply(@"You have been banned."));
                            ctx.Connection.Close();
                            return;
                        }

                        Users.Connect(res, user => {
                            Sessions.HasAvailableSessions(user, available => {
                                // Enforce a maximum amount of connections per user
                                if(!available) {
                                    // map somethign to this
                                    //ctx.Connection.SendPacket(new AuthFailPacket(AuthFailReason.MaxSessions));
                                    ctx.Connection.Close();
                                    return;
                                }

                                Sessions.Create(ctx.Connection, user, session => {
                                    ctx.Connection.SendReply(new WelcomeReply(Server, user));
                                    ctx.Connection.SendReply(new YourHostReply(Server));
                                    ctx.Connection.SendReply(new CreatedReply(Context));
                                    ctx.Connection.SendReply(new MyInfoReply(Server));
                                    ctx.Connection.SendReply(new ISupportReply(Server));

                                    if(WelcomeMessage.HasRandom) {
                                        ctx.Connection.SendReply(new MotdStartReply());

                                        string line = WelcomeMessage.GetRandomString();
                                        if(!string.IsNullOrWhiteSpace(line))
                                            ctx.Connection.SendReply(new MotdReply(line));

                                        ctx.Connection.SendReply(new MotdEndReply());
                                    } else
                                        ctx.Connection.SendReply(new NoMotdReply());

                                    // are these necessary?
                                    ctx.Connection.SendReply(new ListUserClientReply());
                                    ctx.Connection.SendReply(new ListUserOperatorsReply());
                                    ctx.Connection.SendReply(new ListUserUnknownReply());
                                    ctx.Connection.SendReply(new ListUserChannelsReply());
                                    ctx.Connection.SendReply(new ListUserMeReply());

                                    Channels.GetDefaultChannels(channels => {
                                        foreach(IChannel channel in channels)
                                            ChannelUsers.JoinChannel(channel, session);
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
