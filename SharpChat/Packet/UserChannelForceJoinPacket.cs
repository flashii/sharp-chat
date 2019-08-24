﻿using System;
using System.Text;

namespace SharpChat.Packet
{
    public class UserChannelForceJoinPacket : IServerPacket
    {
        public SockChatChannel Channel { get; private set; }

        public UserChannelForceJoinPacket(SockChatChannel channel)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserSwitch);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)SockChatServerMovePacket.ForcedMove);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
