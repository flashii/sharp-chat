using System;

namespace SharpChat {
    /// <summary>
    /// Packet IDs sent from the client to the server.
    /// </summary>
    public enum ClientPacketId {
        /*************
         * VERSION 1 *
         *************/

        /// <summary>
        /// Keep the current session alive and occupied.
        /// </summary>
        Ping = 0,

        /// <summary>
        /// Authenticates the user and creates a session.
        /// </summary>
        Authenticate = 1,

        /// <summary>
        /// Sends a message or a command.
        /// </summary>
        MessageSend = 2,

        /*************
         * VERSION 2 *
         *************/

        /// <summary>
        /// Informs the server which extensions the client supports.
        /// </summary>
        Capabilities = 3,

        /// <summary>
        /// Informs the server that the client is currently typing a message.
        /// </summary>
        Typing = 4,
    }

    /// <summary>
    /// Packet IDs sent from the server to the client.
    /// </summary>
    public enum ServerPacketId {
        /*************
         * VERSION 1 *
         *************/

        /// <summary>
        /// Response to the <see cref="ClientPacketId.Ping"/> packet.
        /// </summary>
        Pong = 0,

        /// <summary>
        /// Both acts as a response to <see cref="ClientPacketId.Authenticate"/> and as a method to inform that a user has connected.
        /// </summary>
        UserConnect = 1,

        /// <summary>
        /// Informs the client of a new message.
        /// </summary>
        MessageAdd = 2,

        /// <summary>
        /// Informs the client that a user has disconnected.
        /// </summary>
        UserDisconnect = 3,

        /// <summary>
        /// Informs the client that a channel may have been added, removed or updated.
        /// </summary>
        ChannelEvent = 4,

        /// <summary>
        /// Informs the client that a user joined or left the channel they are in OR that the client has been forcibly moved to a different channel.
        /// </summary>
        UserMove = 5,

        /// <summary>
        /// Informs the client that a message has been deleted.
        /// </summary>
        MessageDelete = 6,

        /// <summary>
        /// Informs the client about preexisting users, channels and messages.
        /// </summary>
        ContextPopulate = 7,

        /// <summary>
        /// Informs the client that it should clear its user list and/or channel list and/or message list.
        /// </summary>
        ContextClear = 8,

        /// <summary>
        /// Informs the client that they've been kicked or banned.
        /// </summary>
        BAKA = 9,

        /// <summary>
        /// Informs the client that another user has been updated.
        /// </summary>
        UserUpdate = 10,

        /*************
         * VERSION 2 *
         *************/

        /// <summary>
        /// Tells the client what capabilities have been accepted.
        /// </summary>
        CapabilityConfirm = 11,

        /// <summary>
        /// Informs the client that another user is typing.
        /// </summary>
        TypingInfo = 12,

        /// <summary>
        /// Tells the client that it should switch to a different server.
        /// </summary>
        SwitchServer = 13,
    }

    /// <summary>
    /// Actions for <see cref="ServerPacketId.ChannelEvent"/>.
    /// </summary>
    public enum ServerChannelSubPacketId {
        Create = 0,
        Update = 1,
        Delete = 2,
    }

    /// <summary>
    /// Actions for <see cref="ServerPacketId.UserMove"/>.
    /// </summary>
    public enum ServerMoveSubPacketId {
        UserJoined = 0,
        UserLeft = 1,
        ForcedMove = 2,
    }

    /// <summary>
    /// Actions for <see cref="ServerPacketId.ContextPopulate"/>.
    /// </summary>
    public enum ServerContextSubPacketId {
        Users = 0,
        Message = 1,
        Channels = 2,
    }

    /// <summary>
    /// Capability list for <see cref="ClientPacketId.Capabilities"/> and <see cref="ServerPacketId.CapabilityConfirm"/>.
    /// </summary>
    [Flags]
    public enum ClientCapability : int {
        /// <summary>
        /// Supports the typing event.
        /// </summary>
        TYPING = 0x01,

        /// <summary>
        /// Supports being in multiple channels at once.
        /// </summary>
        MCHAN = 0x02,
    }
}
