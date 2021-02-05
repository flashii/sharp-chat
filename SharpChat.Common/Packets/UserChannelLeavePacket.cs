﻿using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class UserChannelLeavePacket : ServerPacketBase {
        public ChatUser User { get; private set; }

        public UserChannelLeavePacket(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMovePacket.UserLeft);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(SequenceId);

            return sb.ToString();
        }
    }
}
