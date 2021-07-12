using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ContextChannelsPacket : ServerPacket {
        public IEnumerable<IChannel> Channels { get; private set; }

        public ContextChannelsPacket(IEnumerable<IChannel> channels) {
            Channels = channels?.Where(c => c != null) ?? throw new ArgumentNullException(nameof(channels));
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextSubPacketId.Channels);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channels.Count());

            foreach(IChannel channel in Channels) {
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(channel.Pack());
            }

            return sb.ToString();
        }
    }
}
