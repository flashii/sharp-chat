﻿using SharpChat.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class ContextMessagePacket : ServerPacket {
        public IChatEvent Event { get; private set; }
        public bool Notify { get; private set; }

        public ContextMessagePacket(IChatEvent evt, bool notify = false) {
            Event = evt ?? throw new ArgumentNullException(nameof(evt));
            Notify = notify;
        }

        private const string V1_CHATBOT = "-1\tChatBot\tinherit\t\t";

        public override IEnumerable<string> Pack(int version) {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextPopulate);
            sb.Append('\t');
            sb.Append((int)SockChatServerContextPacket.Message);
            sb.Append('\t');
            sb.Append(Event.DateTime.ToSockChatSeconds(version));
            sb.Append('\t');

            switch (Event) {
                case IChatMessage msg:
                    sb.Append(Event.Sender.Pack(version));
                    sb.Append('\t');

                    if (version >= 2)
                        sb.Append(msg.Text);
                    else
                        sb.Append(
                            msg.Text
                               .Replace(@"<", @"&lt;")
                               .Replace(@">", @"&gt;")
                               .Replace("\n", @" <br/> ")
                               .Replace("\t", @"    ")
                        );
                    break;

                case UserConnectEvent _:
                    sb.Append(V1_CHATBOT);
                    sb.Append("0\fjoin\f");
                    sb.Append(Event.Sender.Username);
                    break;

                case UserChannelJoinEvent _:
                    sb.Append(V1_CHATBOT);
                    sb.Append("0\fjchan\f");
                    sb.Append(Event.Sender.Username);
                    break;

                case UserChannelLeaveEvent _:
                    sb.Append(V1_CHATBOT);
                    sb.Append("0\flchan\f");
                    sb.Append(Event.Sender.Username);
                    break;

                case UserDisconnectEvent ude:
                    sb.Append(V1_CHATBOT);
                    sb.Append("0\f");

                    switch (ude.Reason) {
                        case UserDisconnectReason.Flood:
                            sb.Append(@"flood");
                            break;
                        case UserDisconnectReason.Kicked:
                            sb.Append(@"kick");
                            break;
                        case UserDisconnectReason.TimeOut:
                            sb.Append(@"timeout");
                            break;
                        case UserDisconnectReason.Leave:
                        default:
                            sb.Append(@"leave");
                            break;
                    }

                    sb.Append('\f');
                    sb.Append(Event.Sender.Username);
                    break;
            }


            sb.Append('\t');
            sb.Append(Event.SequenceId < 1 ? SequenceId : Event.SequenceId);
            sb.Append('\t');
            sb.Append(Notify ? '1' : '0');

            if (version >= 2) {
                sb.Append('\t');
                sb.Append((int)Event.Flags);
            } else {
                sb.AppendFormat(
                    "\t1{0}0{1}{2}",
                    Event.Flags.HasFlag(ChatMessageFlags.Action) ? '1' : '0',
                    Event.Flags.HasFlag(ChatMessageFlags.Action) ? '0' : '1',
                    Event.Flags.HasFlag(ChatMessageFlags.Private) ? '1' : '0'
                );
            }

            return new[] { sb.ToString() };
        }
    }
}
