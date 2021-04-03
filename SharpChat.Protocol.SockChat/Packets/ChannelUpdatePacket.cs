using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Channels;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelUpdatePacket : ServerPacket {
        public string PreviousName { get; private set; }
        public IChannel Channel { get; private set; }

        public ChannelUpdatePacket(string previousName, IChannel channel) {
            PreviousName = previousName;
            Channel = channel;
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelSubPacketId.Update);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(PreviousName);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Pack());

            return sb.ToString();
        }
    }
}
