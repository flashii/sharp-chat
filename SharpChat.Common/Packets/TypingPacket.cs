﻿using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class TypingPacket : ServerPacketBase {
        public Channel Channel { get; }
        public ChannelTyping TypingInfo { get; }

        public TypingPacket(Channel channel, ChannelTyping typingInfo) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            TypingInfo = typingInfo ?? throw new ArgumentNullException(nameof(typingInfo));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.Typing);
            sb.Append('\t');
            sb.Append(Channel.Name);
            sb.Append('\t');
            sb.Append(TypingInfo.User.UserId);
            sb.Append('\t');
            sb.Append(TypingInfo.Started.ToUnixTimeSeconds());

            yield return sb.ToString();
        }
    }
}
