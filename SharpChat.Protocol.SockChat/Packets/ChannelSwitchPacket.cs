using SharpChat.Channels;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelSwitchPacket : ServerPacket {
        public IChannel Channel { get; private set; }

        public ChannelSwitchPacket(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMoveSubPacketId.ForcedMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
