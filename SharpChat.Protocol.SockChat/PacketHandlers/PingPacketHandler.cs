using SharpChat.Sessions;
using System;
using System.Linq;

namespace SharpChat.Protocol.SockChat.PacketHandlers {
    public class PingPacketHandler : IPacketHandler {
        public ClientPacketId PacketId => ClientPacketId.Ping;

        private SessionManager Sessions { get; }

        public PingPacketHandler(SessionManager sessions) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public void HandlePacket(PacketHandlerContext ctx) {
            if(!ctx.HasSession
                && !long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId)
                && ctx.Session.User.UserId != userId)
                return;
            //if(!int.TryParse(ctx.Args.ElementAtOrDefault(2), out int timestamp))
            //    timestamp = -1;

            Sessions.DoKeepAlive(ctx.Session);
        }
    }
}
