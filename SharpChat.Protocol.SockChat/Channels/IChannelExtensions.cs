using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Packets;
using System.Text;

namespace SharpChat.Protocol.SockChat.Channels {
    public static class IChannelExtensions {
        public static string Pack(this IChannel channel) {
            StringBuilder sb = new StringBuilder();
            channel.Pack(sb);
            return sb.ToString();
        }

        public static void Pack(this IChannel channel, StringBuilder sb) {
            sb.Append(channel.Name);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(channel.HasPassword ? '1' : '0');
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(channel.IsTemporary ? '1' : '0');
        }
    }
}
