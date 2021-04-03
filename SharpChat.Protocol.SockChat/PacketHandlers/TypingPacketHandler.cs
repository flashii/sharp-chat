namespace SharpChat.Protocol.SockChat.PacketHandlers {
    public class TypingPacketHandler : IPacketHandler {
        public ClientPacketId PacketId => ClientPacketId.Typing;

        public void HandlePacket(PacketHandlerContext ctx) {
            /*if(!ctx.HasUser)
                return;

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId) || ctx.User.UserId != userId)
                return;

            string channelName = ctx.Args.ElementAtOrDefault(2)?.ToLowerInvariant();
            if(!string.IsNullOrWhiteSpace(channelName))
                return;

            IChannel channel = ctx.User.GetChannels().FirstOrDefault(c => c.Name.ToLowerInvariant() == channelName);
            if(channel == null || !channel.CanType(ctx.User))
                return;

            ctx.Session.LastChannel = channel;

            ChannelTyping info = channel.RegisterTyping(ctx.User);
            if(info == null)
                return;

            channel.SendPacket(new TypingPacket(channel, info));*/
        }
    }
}
