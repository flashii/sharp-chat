﻿namespace SharpChat.Protocol.IRC.Replies {
    public abstract class Reply : IReply {
        public abstract int ReplyCode { get; }
        private string Line { get; set; }

        protected abstract string BuildLine();

        public string GetLine() {
            if(Line == null)
                Line = BuildLine();
            return Line;
        }
    }
}
