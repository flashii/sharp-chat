using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.SockChat.PacketHandlers {
    public class PacketHandlerContext {
        public IEnumerable<string> Args { get; }
        public ISession Session { get; }
        public SockChatConnection Connection { get; }

        public IUser User => Session.User;

        public bool HasSession => Session != null;
        public bool HasUser => HasSession;

        public PacketHandlerContext(IEnumerable<string> args, ISession sess, SockChatConnection conn) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            Session = sess;
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }
    }
}
