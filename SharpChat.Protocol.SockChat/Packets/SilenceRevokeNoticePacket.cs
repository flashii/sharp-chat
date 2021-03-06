﻿using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SilenceRevokeNoticePacket : BotResponsePacket {
        public SilenceRevokeNoticePacket(IUser sender)
            : base(sender, BotArguments.SILENCE_REVOKE_NOTICE, false) { }
    }
}
