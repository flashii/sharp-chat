using SharpChat.Channels;

namespace SharpChat.Protocol.IRC.Channels {
    public static class IChannelExtensions {
        public static string GetIRCName(this IChannel channel) {
            return $@"#{channel.Name.ToLowerInvariant()}"; 
        }

        public static char GetIRCNamesPrefix(this IChannel channel) {
            // maybe?
            //if(channel.IsInviteOnly)
            //    return '*';
            if(channel.HasPassword)
                return '@';
            return '=';
        }
    }
}
