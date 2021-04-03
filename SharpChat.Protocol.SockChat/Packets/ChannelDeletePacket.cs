using SharpChat.Channels;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelDeletePacket : ServerPacket {
        public IChannel Channel { get; private set; }

        public ChannelDeletePacket(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelSubPacketId.Delete);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
