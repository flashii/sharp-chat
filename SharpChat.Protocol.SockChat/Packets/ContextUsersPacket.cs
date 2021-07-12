using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ContextUsersPacket : ServerPacket {
        public IEnumerable<IUser> Users { get; private set; }

        public ContextUsersPacket(IEnumerable<IUser> users) {
            Users = users?.Where(u => u != null) ?? throw new ArgumentNullException(nameof(users));
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextSubPacketId.Users);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Users.Count());

            foreach(IUser user in Users) {
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(user.Pack());
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append('1'); // visibility flag
            }

            return sb.ToString();
        }
    }
}
