using System;

namespace SharpChat.Protocol {
    public class ProtocolException : Exception {
        public ProtocolException(string message) : base(message) { }
    }

    public class ProtocolAlreadyListeningException : ProtocolException {
        public ProtocolAlreadyListeningException() : base(@"Protocol is already listening.") { }
    }
}
