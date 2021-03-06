﻿using SharpChat.Users;

namespace SharpChat.Protocol.SockChat.Packets {
    public class ChannelRankResponsePacket : BotResponsePacket {
        public ChannelRankResponsePacket(IUser sender)
            : base(sender.UserId, BotArguments.CHANNEL_RANK_CHANGED, false) {}
    }
}
