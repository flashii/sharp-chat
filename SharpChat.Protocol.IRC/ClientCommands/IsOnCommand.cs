using SharpChat.Protocol.IRC.Replies;
using SharpChat.Protocol.IRC.Users;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.IRC.ClientCommands {
    public class IsOnCommand : IClientCommand {
        public const string NAME = @"ISON";

        public string CommandName => NAME;
        public bool RequireSession => true;

        private UserManager Users { get; }

        public IsOnCommand(UserManager users) {
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public void HandleCommand(ClientCommandContext ctx) {
            IEnumerable<string> userNames = ctx.Arguments.Select(u => u.ToLowerInvariant());

            const int max_length = 400; // allow for 112 characters of overhead
            int length = 0;
            List<string> batch = new(); 

            void sendBatch() {
                if(length < 1)
                    return;
                ctx.Connection.SendReply(new IsOnReply(batch));
                length = 0;
                batch.Clear();
            };

            Users.GetUsers(u => (u.Status == UserStatus.Online || u.Status == UserStatus.Away) && userNames.Contains(u.GetIRCName()), users => {
                foreach(IUser user in users) {
                    string name = user.GetIRCName();
                    int nameLength = name.Length + 1;

                    if(length + nameLength > max_length)
                        sendBatch();

                    length += nameLength;
                    batch.Add(name);
                }

                sendBatch();
            });
        }
    }
}
