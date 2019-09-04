﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class UserConnectPacket : IServerPacket
    {
        public DateTimeOffset Joined { get; private set; }
        public SockChatUser User { get; private set; }

        public UserConnectPacket(DateTimeOffset joined, SockChatUser user)
        {
            Joined = joined;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserConnect);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Joined.ToUnixTimeSeconds());
            sb.Append(Constants.SEPARATOR);
            sb.Append(User.Pack(version));
            sb.Append(Constants.SEPARATOR);
            sb.Append(eventId);

            return new[] { sb.ToString() };
        }
    }
}
