﻿using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class UserChannelForceJoinPacket : ServerPacketBase {
        public Channel Channel { get; private set; }

        public UserChannelForceJoinPacket(Channel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserSwitch);
            sb.Append('\t');
            sb.Append((int)ServerMovePacket.ForcedMove);
            sb.Append('\t');
            sb.Append(Channel.Name);

            yield return sb.ToString();
        }
    }
}
