using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class AFKCommand : ICommand {
        private const string DEFAULT = @"AFK";
        private const int MAX_LENGTH = 5;

        private UserManager Users { get; }

        public AFKCommand(UserManager users) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"afk";

        public bool DispatchCommand(CommandContext ctx) {
            string statusText = ctx.Args.ElementAtOrDefault(1);
            if(string.IsNullOrWhiteSpace(statusText))
                statusText = DEFAULT;
            else {
                statusText = statusText.Trim();
                if(statusText.Length > MAX_LENGTH)
                    statusText = statusText.Substring(0, MAX_LENGTH).Trim();
            }

            Users.Update(ctx.User, status: UserStatus.Away, statusMessage: statusText);
            return true;
        }
    }
}
