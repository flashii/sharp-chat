using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class BotResponsePacket : ServerPacket {
        private IChannel Channel { get; }
        private long UserId { get; }
        private BotArguments Arguments { get; }
        private DateTimeOffset DateTime { get; }
        private long ArbitraryId { get; }

        public BotResponsePacket(IUser sender, string stringId, bool isError = true, params object[] args)
            : this(sender?.UserId ?? throw new ArgumentNullException(nameof(sender)), stringId, isError, args) { }

        public BotResponsePacket(long userId, string stringId, bool isError = true, params object[] args)
            : this(null, userId, stringId, isError, args) { }

        public BotResponsePacket(IChannel channel, long userId, string stringId, bool isError = true, params object[] args)
            : this(channel, userId, new BotArguments(isError, stringId, args)) { }

        public BotResponsePacket(IUser sender, BotArguments args)
            : this(null, sender?.UserId ?? throw new ArgumentNullException(nameof(sender)), args) { }

        public BotResponsePacket(long userId, BotArguments args)
            : this(null, userId, args) { }

        public BotResponsePacket(IChannel channel, long userid, BotArguments args) {
            Arguments = args ?? throw new ArgumentNullException(nameof(args));
            UserId = userid;
            Channel = channel;
            DateTime = DateTimeOffset.Now;
            ArbitraryId = SharpId.Next();
        }

        protected override string DoPack() {
            StringBuilder sb = new();

            sb.Append((int)ServerPacketId.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(UserId);
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
