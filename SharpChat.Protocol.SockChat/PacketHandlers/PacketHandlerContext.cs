using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.SockChat.PacketHandlers {
    public class PacketHandlerContext {
        public IEnumerable<string> Args { get; }
        public SockChatConnection Connection { get; }

        public ISession Session => Connection.Session;
        public IUser User => Session.User;

        public bool HasSession => Session != null;
        public bool HasUser => HasSession;

        public PacketHandlerContext(IEnumerable<string> args, SockChatConnection conn) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }
    }
}
