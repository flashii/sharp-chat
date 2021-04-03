using SharpChat.Users;
using System.Net;

namespace SharpChat.Protocol.SockChat.Packets {
    public class PardonResponsePacket : BotResponsePacket {
        public PardonResponsePacket(IUser sender, string userName)
            : base(sender, BotArguments.Notice(@"unban", userName)) { }

        public PardonResponsePacket(IUser sender, IPAddress ipAddr)
            : base(sender, BotArguments.Notice(@"unban", ipAddr)) { }
    }
}
