using SharpChat.DataProvider;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Protocol.SockChat.Commands {
    public class PardonIPCommand : ICommand {
        private IDataProvider DataProvider { get; }
        private IUser Sender { get; }

        public PardonIPCommand(IDataProvider dataProvider, IUser sender) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name is @"pardonip" or @"unbanip";

        public bool DispatchCommand(CommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser)) {
                ctx.Connection.SendPacket(new CommandNotAllowedErrorPacket(Sender, ctx.Args));
                return true;
            }

            string ipAddress = ctx.Args.ElementAtOrDefault(1);
            if(!IPAddress.TryParse(ipAddress, out IPAddress ipAddr)) {
                ctx.Connection.SendPacket(new NotBannedErrorPacket(Sender, ipAddr?.ToString() ?? @"::"));
                return true;
            }

            DataProvider.BanClient.RemoveBan(ipAddr, success => {
                if(success)
                    ctx.Connection.SendPacket(new PardonResponsePacket(Sender, ipAddr));
                else
                    ctx.Connection.SendPacket(new NotBannedErrorPacket(Sender, ipAddr.ToString()));
            }, ex => ctx.Connection.SendPacket(new GenericErrorPacket(Sender)));

            return true;
        }
    }
}
