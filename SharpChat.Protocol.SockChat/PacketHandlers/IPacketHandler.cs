namespace SharpChat.Protocol.SockChat.PacketHandlers {
    public interface IPacketHandler {
        ClientPacketId PacketId { get; }
        void HandlePacket(PacketHandlerContext ctx);
    }
}
