using SharpChat.Bans;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Packets {
    public class BanListPacket : BotResponsePacket {
        private const string FORMAT = @"<a href=""javascript:void(0);"" onclick=""Chat.SendMessageWrapper('/unban '+ this.innerHTML);"">{0}</a>, ";

        public BanListPacket(IUser sender, IEnumerable<IBanRecord> bans)
            : base(
                sender.UserId,
                BotArguments.BANS,
                false,
                string.Join(@", ", bans.Select(b => string.Format(FORMAT, b.Username)))
            ) { }
    }
}
