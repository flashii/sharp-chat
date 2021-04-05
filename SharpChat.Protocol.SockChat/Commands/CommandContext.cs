using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.SockChat.Commands {
    public class CommandContext {
        public IEnumerable<string> Args { get; }
        public IUser User { get; }
        public IChannel Channel { get; }
        public ISession Session { get; }
        public SockChatConnection Connection { get; }

        public CommandContext(
            IEnumerable<string> args,
            IUser user,
            IChannel channel,
            ISession session,
            SockChatConnection connection
        ) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }
    }
}
