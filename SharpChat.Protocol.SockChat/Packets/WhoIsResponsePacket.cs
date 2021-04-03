using SharpChat.Users;
using System.Net;

namespace SharpChat.Protocol.SockChat.Packets {
    public class WhoIsResponsePacket : BotResponsePacket {
        public WhoIsResponsePacket(IUser sender, string userName, IPAddress ipAddress)
            : base(sender, BotArguments.Notice(@"ipaddr", userName, ipAddress)) { }

        public WhoIsResponsePacket(IUser sender, IUser user, IPAddress ipAddress)
            : this(sender, user.UserName, ipAddress) { }
    }
}
