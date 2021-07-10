using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Replies;
using SharpChat.Protocol.IRC.ServerCommands;
using SharpChat.Protocol.IRC.Sessions;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class ModeCommand : IClientCommand {
        public const string NAME = @"MODE";

        public string CommandName => NAME;
        public bool RequireSession => true;

        private ChannelManager Channels { get; }
        private UserManager Users { get; }
        private SessionManager Sessions { get; }

        public ModeCommand(ChannelManager channels, UserManager users, SessionManager sessions) {
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            string targetName = ctx.Arguments.ElementAtOrDefault(0);
            if(string.IsNullOrWhiteSpace(targetName)) {
                ctx.Connection.SendReply(new NeedMoreParamsReply(NAME));
                return;
            }

            if(!ctx.Arguments.Any()) {
                ctx.Connection.SendReply(new NeedMoreParamsReply(NAME));
                return;
            }

            if(targetName.StartsWith('#'))
                Channels.GetChannelByIRCName(targetName, channel => {
                    if(channel == null) {
                        ctx.Connection.SendReply(new NoSuchChannelReply(targetName));
                        return;
                    }

                    if(ctx.Arguments.Count() == 1) {
                        //ctx.Connection.SendCommand(new ServerModeCommand(channel));
                        return;
                    }

                    // owner check

                    HandleChannel(ctx, channel);
                });
            else
                Users.GetUser(targetName, user => {
                    if(user == null) {
                        ctx.Connection.SendReply(new NoSuchNickReply(targetName));
                        return;
                    }

                    if(ctx.Arguments.Count() == 1) {
                        //Sessions.CheckIRCSecure(user, isSecure => ctx.Connection.SendCommand(new ServerModeCommand(user, isSecure)));
                        return;
                    }

                    if(!user.Equals(ctx.User)) {
                        // admin check probably
                        ctx.Connection.SendReply(new UsersDoNotMatchReply());
                        return;
                    }

                    HandleUser(ctx, user);
                });
        }

        private void HandleChannel(ClientCommandContext ctx, IChannel channel) {
            Queue<string> args = new(ctx.Arguments);

            while(args.TryDequeue(out string arg)) {
                //
            }
        }

        private void HandleUser(ClientCommandContext ctx, IUser user) {
            HashSet<char> processed = new();

            string modeSet = ctx.Arguments.FirstOrDefault();
            if(modeSet.Length < 2)
                return;

            Queue<char> chars = new(modeSet.ToArray());

            char mode = chars.Dequeue();
            if(mode is not '+' and not '-')
                return;

            bool set = mode == '+';

            while(chars.TryDequeue(out mode)) {
                if(processed.Contains(mode))
                    continue;
                processed.Add(mode);

                switch(mode) {
                    case 'i': // Invisible (appear offline)
                        Users.Update(user, status: set ? UserStatus.Offline : UserStatus.Online);
                        break;

                    default:
                        ctx.Connection.SendReply(new UserModeUnknownFlagReply());
                        chars.Clear();
                        return;
                }
            }

            Sessions.CheckIRCSecure(user, isSecure => ctx.Connection.SendReply(new UserModeIsReply(user, isSecure)));
        }
    }
}
