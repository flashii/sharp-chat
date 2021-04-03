using SharpChat.Bans;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Packets {
    public class BanListPacket : BotResponsePacket {
        public BanListPacket(IUser sender, IEnumerable<IBanRecord> bans)
            : base(sender, BotArguments.Notice(@"banlist", string.Join(@", ", bans.Select(
                b => string.Format(@"<a href=""javascript:void(0);"" onclick=""Chat.SendMessageWrapper('/unban '+ this.innerHTML);"">{0}</a>, ", b.Username)
            )))) { }
    }
}
