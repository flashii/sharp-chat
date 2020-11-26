﻿namespace SharpChat {
    public enum SockChatClientPacket {
        // Version 1
        Ping = 0,
        Authenticate = 1,
        MessageSend = 2,

        // Version 2
        Typing = 3,
    }

    public enum SockChatServerPacket {
        // Version 1
        Pong = 0,
        UserConnect = 1,
        MessageAdd = 2,
        UserDisconnect = 3,
        ChannelEvent = 4,
        UserSwitch = 5,
        MessageDelete = 6,
        ContextPopulate = 7,
        ContextClear = 8,
        BAKA = 9,
        UserUpdate = 10,

        // Version 2
        Typing = 11,
    }

    public enum SockChatServerChannelPacket {
        Create = 0,
        Update = 1,
        Delete = 2,
    }

    public enum SockChatServerMovePacket {
        UserJoined = 0,
        UserLeft = 1,
        ForcedMove = 2,
    }

    public enum SockChatServerContextPacket {
        Users = 0,
        Message = 1,
        Channels = 2,
    }
}