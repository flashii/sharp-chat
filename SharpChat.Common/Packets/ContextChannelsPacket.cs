﻿using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packets {
    public class ContextChannelsPacket : ServerPacket {
        public IEnumerable<Channel> Channels { get; private set; }

        public ContextChannelsPacket(IEnumerable<Channel> channels) {
            Channels = channels?.Where(c => c != null) ?? throw new ArgumentNullException(nameof(channels));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextPopulate);
            sb.Append('\t');
            sb.Append((int)SockChatServerContextPacket.Channels);
            sb.Append('\t');
            sb.Append(Channels.Count());

            foreach (Channel channel in Channels) {
                sb.Append('\t');
                sb.Append(channel.Pack());
            }

            yield return sb.ToString();
        }
    }
}
