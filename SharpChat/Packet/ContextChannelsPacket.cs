﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packet
{
    public class ContextChannelsPacket : ServerPacket
    {
        public IEnumerable<SockChatChannel> Channels { get; private set; }

        public ContextChannelsPacket(IEnumerable<SockChatChannel> channels)
        {
            Channels = channels?.Where(c => c != null) ?? throw new ArgumentNullException(nameof(channels));
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextPopulate);
            sb.Append('\t');
            sb.Append((int)SockChatServerContextPacket.Channels);
            sb.Append('\t');
            sb.Append(Channels.Count());

            foreach (SockChatChannel channel in Channels)
            {
                sb.Append('\t');
                sb.Append(channel.Pack(version));
            }

            return new[] { sb.ToString() };
        }
    }
}
