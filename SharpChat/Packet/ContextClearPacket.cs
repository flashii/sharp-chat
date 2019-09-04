﻿using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public enum ContextClearMode
    {
        Messages = 0,
        Users = 1,
        Channels = 2,
        MessagesUsers = 3,
        MessagesUsersChannels = 4,
    }

    public class ContextClearPacket : IServerPacket
    {
        public ContextClearMode Mode { get; private set; }

        public ContextClearPacket(ContextClearMode mode)
        {
            Mode = mode;
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            if (version > 1)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextClear);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)Mode);

            return new[] { sb.ToString() };
        }
    }
}
