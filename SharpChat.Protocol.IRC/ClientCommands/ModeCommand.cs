﻿using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Replies;
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

            if(ctx.Arguments.Count() < 2) {
                ctx.Connection.SendReply(new NeedMoreParamsReply(NAME));
                return;
            }

            if(targetName.StartsWith('#'))
                Channels.GetChannelByIRCName(targetName, channel => {
                    if(channel == null) {
                        ctx.Connection.SendReply(new NoSuchChannelReply(targetName));
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

                    if(!user.Equals(ctx.User)) {
                        // admin check probably
                        ctx.Connection.SendReply(new UsersDoNotMatchReply());
                        return;
                    }

                    HandleUser(ctx, user);
                });
        }

        private void HandleChannel(ClientCommandContext ctx, IChannel channel) {
            // TODO: CHANNEL MODES
        }

        private void HandleUser(ClientCommandContext ctx, IUser user) {
            HashSet<char> processed = new HashSet<char>();

            foreach(string modeSet in ctx.Arguments.Skip(1)) {
                if(modeSet.Length < 2)
                    continue;

                Queue<char> chars = new Queue<char>(modeSet.ToArray());

                char mode = chars.Dequeue();
                if(mode != '+' && mode != '-')
                    continue;

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
                            processed = null;
                            break;
                    }
                }

                if(processed == null)
                    break;
            }

            Sessions.CheckIRCSecure(user, isSecure => ctx.Connection.SendReply(new UserModeIsReply(user, isSecure)));
        }
    }
}
