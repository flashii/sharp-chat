using SharpChat.Users;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class AwayCommand : IClientCommand {
        public const string NAME = @"AWAY";

        public string CommandName => NAME;
        public bool RequireSession => true;

        private UserManager Users { get; }

        public AwayCommand(UserManager users) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            string line = ctx.Arguments.FirstOrDefault() ?? string.Empty;
            bool isAway = !string.IsNullOrEmpty(line);

            Users.Update(
                ctx.User,
                status: isAway ? UserStatus.Away : UserStatus.Online,
                statusMessage: line
            );
        }
    }
}
