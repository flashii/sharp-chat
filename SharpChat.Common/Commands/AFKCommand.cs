﻿using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Linq;

namespace SharpChat.Commands {
    public class AFKCommand : IChatCommand {
        private const string DEFAULT = @"AFK";
        private const int MAX_LENGTH = 5;

        public bool IsMatch(string name)
            => name == @"afk";

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            string statusText = ctx.Args.ElementAtOrDefault(1);
            if(string.IsNullOrWhiteSpace(statusText))
                statusText = DEFAULT;
            else {
                statusText = statusText.Trim();
                if(statusText.Length > MAX_LENGTH)
                    statusText = statusText.Substring(0, MAX_LENGTH).Trim();
            }

            ctx.User.Status = ChatUserStatus.Away;
            ctx.User.StatusMessage = statusText;
            ctx.Channel.Send(new UserUpdatePacket(ctx.User));
            return null;
        }
    }
}
