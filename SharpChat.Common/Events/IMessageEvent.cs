﻿namespace SharpChat.Events {
    public interface IMessageEvent : IEvent {
        string Text { get; }
        bool IsAction { get; }
    }
}
