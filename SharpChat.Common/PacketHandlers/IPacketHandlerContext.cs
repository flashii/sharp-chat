﻿using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;

namespace SharpChat.PacketHandlers {
    public interface IPacketHandlerContext {
        IEnumerable<string> Args { get; }
        ChatContext Chat { get; }
        IWebSocketConnection Connection { get; }
        Session Session { get; }
        ChatUser User { get; }

        bool HasSession { get; }
        bool HasUser { get; }
    }

    public class PacketHandlerContext : IPacketHandlerContext {
        public IEnumerable<string> Args { get; }
        public ChatContext Chat { get; }
        public Session Session { get; }
        public IWebSocketConnection Connection { get; }

        public ChatUser User => Session.User;

        public bool HasSession => Session != null;
        public bool HasUser => HasSession;

        public PacketHandlerContext(IEnumerable<string> args, ChatContext ctx, Session sess, IWebSocketConnection conn) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            Chat = ctx ?? throw new ArgumentNullException(nameof(ctx));
            Session = sess;
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }
    }
}
