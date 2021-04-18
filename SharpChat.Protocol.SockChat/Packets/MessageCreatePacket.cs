using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class MessageCreatePacket : ServerPacket {
        private IEvent Event { get; }
        private string Text { get; }
        private bool IsAction { get; }

        public MessageCreatePacket(MessageCreateEvent create)
            : this(create, create.Text, create.IsAction) { }

        private MessageCreatePacket(IEvent evt, string text, bool isAction) {
            Event = evt ?? throw new ArgumentNullException(nameof(evt));
            IsAction = isAction;

            StringBuilder sb = new StringBuilder();

            if(isAction)
                sb.Append(@"<i>");

            sb.Append(
                text
                    .Replace(@"<", @"&lt;")
                    .Replace(@">", @"&gt;")
                    .Replace("\n", @" <br/> ")
                    .Replace("\t", @"    ")
            );

            if(isAction)
                sb.Append(@"</i>");

            Text = sb.ToString();
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Event.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Event.UserId.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Text);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Event.EventId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.AppendFormat(
                "1{0}0{1}{2}",
                IsAction ? '1' : '0',
                IsAction ? '0' : '1',
                /*Flags.HasFlag(EventFlags.Private)*/ false ? '1' : '0'
            );
            sb.Append(IServerPacket.SEPARATOR);
            if(!Event.IsBroadcast())
                sb.Append(Event.ChannelName.Name);

            return sb.ToString();
        }
    }
}
