﻿using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class UserConnectPacket : ServerPacketBase {
        public DateTimeOffset Joined { get; private set; }
        public ChatUser User { get; private set; }

        public UserConnectPacket(DateTimeOffset joined, ChatUser user) {
            Joined = joined;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserConnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Joined.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.Pack());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(SequenceId);

            yield return sb.ToString();
        }
    }
}
