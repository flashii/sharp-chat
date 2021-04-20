using SharpChat.Channels;
using System;

namespace SharpChat.Protocol.IRC.Channels {
    public static class ChannelManagerExtensions {
        public static void GetChannelByIRCName(this ChannelManager channels, string ircName, Action<IChannel> callback) {
            if(ircName == null)
                throw new ArgumentNullException(nameof(ircName));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(string.IsNullOrWhiteSpace(ircName) || ircName == @"#") {
                callback(null);
                return;
            }
            channels.GetChannel(c => ircName.Equals(c.GetIRCName()), callback);
        }
    }
}
