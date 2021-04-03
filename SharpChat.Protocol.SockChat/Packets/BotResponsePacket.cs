using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class BotResponsePacket : ServerPacket {
        private IChannel Channel { get; }
        private IUser Sender { get; }
        private BotArguments Arguments { get; }
        private DateTimeOffset DateTime { get; }
        private long ArbitraryId { get; }

        public BotResponsePacket(IUser sender, string stringId, bool isError = true, params object[] args)
            : this(null, sender, stringId, isError, args) { }

        public BotResponsePacket(IChannel channel, IUser user, string stringId, bool isError = true, params object[] args)
            : this(channel, user, new BotArguments(isError, stringId, args)) { }

        public BotResponsePacket(IUser sender, BotArguments args)
            : this(null, sender, args) {}

        public BotResponsePacket(IChannel channel, IUser sender, BotArguments args) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Arguments = args ?? throw new ArgumentNullException(nameof(args));
            Channel = channel;
            DateTime = DateTimeOffset.Now;
            ArbitraryId = SharpId.Next();
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Sender.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Arguments);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(ArbitraryId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.AppendFormat(@"10010");
            sb.Append(IServerPacket.SEPARATOR);
            if(Channel != null)
                sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
