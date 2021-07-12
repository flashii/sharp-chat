using SharpChat.Protocol.SockChat.Users;
using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilenceRevokeResponsePacket : BotResponsePacket {
        public SilenceRevokeResponsePacket(IUser sender, string userName)
            : base(sender, BotArguments.SILENCE_REVOKE_CONFIRM, false, userName) { }

        public SilenceRevokeResponsePacket(IUser sender, IUser target)
            : this(sender, target.GetDisplayName()) { }
    }
}
