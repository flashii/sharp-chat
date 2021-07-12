using SharpChat.DataProvider;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.SockChat.Commands {
    public class BanListCommand : ICommand {
        private IDataProvider DataProvider { get; }
        private IUser Sender { get; }

        public BanListCommand(IDataProvider dataProvider, IUser sender) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name is @"bans" or @"banned";

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser)) {
                ctx.Connection.SendPacket(new CommandNotAllowedErrorPacket(Sender, ctx.Args));
                return true;
            }

            DataProvider.BanClient.GetBanList(b => {
                ctx.Connection.SendPacket(new BanListPacket(Sender, b));
            }, ex => {
                Logger.Write(@"Error during ban list retrieval.");
                Logger.Write(ex);
                ctx.Connection.SendPacket(new GenericErrorPacket(Sender));
            });

            return true;
        }
    }
}
