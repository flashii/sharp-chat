using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class WelcomeMessagePacket : ServerPacket {
        private IUser Sender { get; }
        private string Message { get; }

        public WelcomeMessagePacket(IUser sender, string message) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextSubPacketId.Message);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Sender.Pack());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(new BotArguments(BotArguments.WELCOME, false, Message));
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(BotArguments.WELCOME);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append('0');
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(@"10010");

            return sb.ToString();
        }
    }
}
