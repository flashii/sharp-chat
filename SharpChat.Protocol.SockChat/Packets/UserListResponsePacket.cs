using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class UserListResponsePacket : BotResponsePacket {
        public UserListResponsePacket(IUser sender, IUser requester, IEnumerable<IUser> users)
            : base(sender, BotArguments.Notice(@"who", MakeUserList(requester, users))) { }

        public UserListResponsePacket(IUser sender, IChannel channel, IUser requester, IEnumerable<IUser> users)
            : this(sender, channel.Name, requester, users) { }

        public UserListResponsePacket(IUser sender, string channelName, IUser requester, IEnumerable<IUser> users)
            : base(sender, BotArguments.Notice(@"whochan", channelName, MakeUserList(requester, users))) { }

        private static string MakeUserList(IUser requester, IEnumerable<IUser> users) {
            StringBuilder sb = new();

            foreach(IUser user in users) {
                sb.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                if(user == requester)
                    sb.Append(@" style=""font-weight: bold;""");

                sb.Append('>');
                sb.Append(user.GetDisplayName());
                sb.Append(@"</a>, ");
            }

            if(sb.Length > 2)
                sb.Length -= 2;

            return sb.ToString();
        }
    }
}
