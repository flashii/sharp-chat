﻿using SharpChat.Channels;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelCreatePacket : ServerPacket {
        public Channel Channel { get; private set; }

        public ChannelCreatePacket(Channel channel) {
            Channel = channel;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ChannelEvent);
            sb.Append('\t');
            sb.Append((int)SockChatServerChannelPacket.Create);
            sb.Append('\t');
            sb.Append(Channel.Pack());

            yield return sb.ToString();
        }
    }
}
