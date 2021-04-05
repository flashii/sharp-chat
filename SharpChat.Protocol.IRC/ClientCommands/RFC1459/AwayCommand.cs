using SharpChat.Users;
using System;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands.RFC1459 {
    public class AwayCommand : IClientCommand {
        public const string NAME = @"AWAY";

        public string CommandName => NAME;

        private UserManager Users { get; }

        public AwayCommand(UserManager users) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            if(!ctx.HasUser)
                return;

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
