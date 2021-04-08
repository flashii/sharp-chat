using SharpChat.DataProvider;
using SharpChat.Protocol.IRC.Replies;
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

        private const string WELCOME = @"welcome.txt";

        public string CommandName => NAME;

        private UserManager Users { get; }
        private SessionManager Sessions { get; }
        private IDataProvider DataProvider { get; }

        public UserCommand(UserManager users, SessionManager sessions, IDataProvider dataProvider) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
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

                    Logger.Debug(@"here 1");

                    DataProvider.BanClient.CheckBan(res.UserId, ctx.Connection.RemoteAddress, ban => {
                        if(ban.IsPermanent || ban.Expires > DateTimeOffset.Now) {
                            // should probably include the time

                            ctx.Connection.SendReply(new YouAreBannedReply(@"You have been banned."));
                            ctx.Connection.Close();
                            return;
                        }

                        Logger.Debug(@"here 2");

                        IUser user = Users.Connect(res);
                        Logger.Debug(@"here 3");

                        // Enforce a maximum amount of connections per user
                        if(Sessions.GetAvailableSessionCount(user) < 1) {
                            // map somethign to this
                            //ctx.Connection.SendPacket(new AuthFailPacket(AuthFailReason.MaxSessions));
                            ctx.Connection.Close();
                            return;
                        }

                        Logger.Debug(@"here 4");
                        ISession sess = Sessions.Create(ctx.Connection, user);
                        Logger.Debug(@"here 5");

                        ctx.Connection.SendReply(new WelcomeReply());
                        ctx.Connection.SendReply(new YourHostReply());
                        ctx.Connection.SendReply(new CreatedReply());
                        ctx.Connection.SendReply(new MyInfoReply());
                        ctx.Connection.SendReply(new ISupportReply());

                        if(File.Exists(WELCOME)) {
                            ctx.Connection.SendReply(new MotdStartReply());

                            IEnumerable<string> lines = File.ReadAllLines(WELCOME).Where(x => !string.IsNullOrWhiteSpace(x));
                            string line = lines.ElementAtOrDefault(RNG.Next(lines.Count()));
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
                    }, exceptionHandler);
                },
                exceptionHandler
            );
        }
    }
}
