﻿using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public enum UserDisconnectReason {
        Leave,
        TimeOut,
        Kicked,
        Flood,
    }

    public class UserDisconnectPacket : ServerPacket {
        public DateTimeOffset Disconnected { get; private set; }
        public ChatUser User { get; private set; }
        public UserDisconnectReason Reason { get; private set; }

        public UserDisconnectPacket(DateTimeOffset disconnected, ChatUser user, UserDisconnectReason reason) {
            Disconnected = disconnected;
            User = user ?? throw new ArgumentNullException(nameof(user));
            Reason = reason;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserDisconnect);
            sb.Append('\t');
            sb.Append(User.UserId);
            sb.Append('\t');
            sb.Append(User.DisplayName);
            sb.Append('\t');

            switch (Reason) {
                case UserDisconnectReason.Leave:
                default:
                    sb.Append(@"leave");
                    break;
                case UserDisconnectReason.TimeOut:
                    sb.Append(@"timeout");
                    break;
                case UserDisconnectReason.Kicked:
                    sb.Append(@"kick");
                    break;
                case UserDisconnectReason.Flood:
                    sb.Append(@"flood");
                    break;
            }

            sb.Append('\t');
            sb.Append(Disconnected.ToUnixTimeSeconds());
            sb.Append('\t');
            sb.Append(SequenceId);

            return new[] { sb.ToString() };
        }
    }
}
