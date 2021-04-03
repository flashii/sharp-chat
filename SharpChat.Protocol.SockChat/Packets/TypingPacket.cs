using SharpChat.Channels;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class TypingPacket : ServerPacket {
        public IChannel Channel { get; }
        public object TypingInfo { get; }

        public TypingPacket(IChannel channel, object typingInfo) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            TypingInfo = typingInfo ?? throw new ArgumentNullException(nameof(typingInfo));
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.TypingInfo);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);
            sb.Append(IServerPacket.SEPARATOR);
            //sb.Append(TypingInfo.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            //sb.Append(TypingInfo.Started.ToUnixTimeSeconds());

            return sb.ToString();
        }
    }
}
