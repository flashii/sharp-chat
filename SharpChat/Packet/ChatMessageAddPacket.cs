﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class ChatMessageAddPacket : IServerPacket
    {
        public IChatMessage Message { get; private set; }

        public ChatMessageAddPacket(IChatMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageAdd);
            sb.Append(Constants.SEPARATOR);

            if(version >= 2)
            {
                sb.Append(Message.Channel?.Name ?? @"@broadcast");
                sb.Append(Constants.SEPARATOR);
            }

            sb.Append(Message.DateTime.ToUnixTimeSeconds());
            sb.Append(Constants.SEPARATOR);

            sb.Append(Message.User?.UserId ?? -1);
            sb.Append(Constants.SEPARATOR);

            if (version >= 2)
                sb.Append(Message.Text);
            else
            {
                if (Message.Flags == SockChatMessageFlags.Action)
                    sb.Append(@"<i>");

                sb.Append(
                    Message.Text
                        .Replace(@"<", @"&lt;")
                        .Replace(@">", @"&gt;")
                        .Replace("\n", @" <br/> ")
                        .Replace("\t", @"    ")
                );

                if (Message.Flags == SockChatMessageFlags.Action)
                    sb.Append(@"</i>");
            }

            sb.Append(Constants.SEPARATOR);
            sb.Append(eventId);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Message.Flags.Serialise());

            return new[] { sb.ToString() };
        }
    }
}
