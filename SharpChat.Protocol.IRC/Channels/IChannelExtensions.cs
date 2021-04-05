using SharpChat.Channels;
using System.Text;

namespace SharpChat.Protocol.IRC.Channels {
    public static class IChannelExtensions {
        public static string GetIRCName(this IChannel channel) {
            StringBuilder sb = new StringBuilder();

            // expand this when there's more channel types
            sb.Append('#');
            sb.Append(channel.Name.ToLowerInvariant());

            return sb.ToString(); 
        }
    }
}
