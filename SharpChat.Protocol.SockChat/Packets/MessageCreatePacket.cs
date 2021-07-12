using SharpChat.Channels;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class MessageCreatePacket : ServerPacket {
        private long MessageId { get; }
        private long UserId { get; }
        private DateTimeOffset DateTime { get; }
        private IChannel Channel { get; }
        private string Text { get; }
        private bool IsAction { get; }

        public MessageCreatePacket(long messageId, long userId, DateTimeOffset dateTime, IChannel channel, string text, bool isAction) {
            MessageId = messageId;
            UserId = userId;
            DateTime = dateTime;
            Channel = channel;
            IsAction = isAction;

            StringBuilder sb = new();

            if(isAction)
                sb.Append(@"<i>");

            sb.Append(text.CleanTextForMessage());

            if(isAction)
                sb.Append(@"</i>");

            Text = sb.ToString();
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Text);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(MessageId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.AppendFormat(
                "1{0}0{1}{2}",
                IsAction ? '1' : '0',
                IsAction ? '0' : '1',
                /*Flags.HasFlag(EventFlags.Private)*/ false ? '1' : '0'
            );
            sb.Append(IServerPacket.SEPARATOR);
            if(Channel == null) // broadcast
                sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
