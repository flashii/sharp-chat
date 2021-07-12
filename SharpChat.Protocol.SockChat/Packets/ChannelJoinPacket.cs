using SharpChat.Events;
using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelJoinPacket : ServerPacket {
        private ChannelUserJoinEvent Join { get; }

        private IUser User { get; }

        public ChannelJoinPacket(ChannelUserJoinEvent join, IUser user) {
            Join = join ?? throw new ArgumentNullException(nameof(join));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMoveSubPacketId.UserJoined);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.Colour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.EventId);

            return sb.ToString();
        }
    }
}
