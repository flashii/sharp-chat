﻿using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Net;

namespace SharpChat.Sessions {
    public interface ISession : IEventHandler, IEquatable<ISession> {
        string SessionId { get; }
        string ServerId { get; }
        DateTimeOffset LastPing { get; }
        IUser User { get; }
        bool IsConnected { get; }
        IPAddress RemoteAddress { get; }
        ClientCapability Capabilities { get; }
    }
}
