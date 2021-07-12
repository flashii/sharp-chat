using SharpChat.Messages;
using SharpChat.Protocol.SockChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ContextMessagePacket : ServerPacket {
        public IMessage Message { get; private set; }
        public bool Notify { get; private set; }

        public ContextMessagePacket(IMessage msg, bool notify = false) {
            Message = msg ?? throw new ArgumentNullException(nameof(msg));
            Notify = notify;
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextSubPacketId.Message);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Message.Created.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);

            sb.Append(Message.Sender.Pack());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Message.GetSanitisedText());

            /*switch (Event) {
                case MessageCreateEvent msg:
                    sb.Append(Event.Sender.Pack());
                    sb.Append(IServerPacket.SEPARATOR);
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
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(BotArguments.Notice(@"join", Event.Sender.UserName));
                    break;

                case ChannelJoinEvent _:
                    sb.Append(V1_CHATBOT);
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(BotArguments.Notice(@"jchan", Event.Sender.UserName));
                    break;

                case ChannelLeaveEvent _:
                    sb.Append(V1_CHATBOT);
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(BotArguments.Notice(@"lchan", Event.Sender.UserName));
                    break;

                case UserDisconnectEvent ude:
                    string udeReason = ude.Reason switch {
                        UserDisconnectReason.Flood => @"flood",
                        UserDisconnectReason.Kicked => @"kick",
                        UserDisconnectReason.TimeOut => @"timeout",
                        _ => @"leave",
                    };

                    sb.Append(V1_CHATBOT);
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(BotArguments.Notice(udeReason, Event.Sender.UserName));
                    break;
            }*/

            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Message.MessageId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Notify ? '1' : '0');
            sb.Append(IServerPacket.SEPARATOR);
            sb.AppendFormat(
                "1{0}0{1}{2}",
                Message.IsAction ? '1' : '0',
                Message.IsAction ? '0' : '1',
                /*Event.Flags.HasFlag(EventFlags.Private)*/ false ? '1' : '0'
            );

            return sb.ToString();
        }
    }
}
