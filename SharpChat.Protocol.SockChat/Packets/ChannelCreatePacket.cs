using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Channels;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelCreatePacket : ServerPacket {
        public IChannel Channel { get; private set; }

        public ChannelCreatePacket(IChannel channel) {
            Channel = channel;
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelSubPacketId.Create);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Pack());

            return sb.ToString();
        }
    }
}
