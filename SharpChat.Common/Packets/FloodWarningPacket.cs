﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class FloodWarningPacket : ServerPacket {
        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageAdd);
            sb.Append('\t');
            sb.Append(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            sb.Append("\t-1\t0\fflwarn\t");
            sb.Append(SequenceId);
            sb.Append("\t10010");

            yield return sb.ToString();
        }
    }
}
