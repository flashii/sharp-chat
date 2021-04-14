using SharpChat.Channels;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

// NOT SURE why i thought this was a good idea
// revert this back to just shooting packets

namespace SharpChat.Protocol.SockChat.Commands {
    public class CommandException : Exception {
        private string ErrorCode { get; }
        private object[] Arguments { get; }

        public CommandException(string errorCode, params object[] args) : base(errorCode) {
            ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
            Arguments = args;
        }

        public IServerPacket ToPacket(IUser sender) {
            return new BotResponsePacket(sender, BotArguments.Error(ErrorCode, Arguments));
        }
    }

    public class CommandGenericException : CommandException {
        public CommandGenericException() : base(@"generr") { }
    }

    public class CommandNotFoundException : CommandException {
        public CommandNotFoundException(string commandName) : base(@"nocmd", commandName) { }
    }

    public class CommandNotAllowedException : CommandException {
        public CommandNotAllowedException(string commandName) : base(@"cmdna", @"/" + commandName) { }
        public CommandNotAllowedException(IEnumerable<string> argList) : this(argList.First()) { }
    }

    public class CommandFormatException : CommandException {
        public CommandFormatException() : base(@"cmderr") { }
    }

    public class UserNotFoundCommandException : CommandException {
        public UserNotFoundCommandException(string userName) : base(@"usernf", userName ?? @"User") { }
    }

    public class SelfSilenceCommandException : CommandException {
        public SelfSilenceCommandException() : base(@"silself") { }
    }

    public class SilenceNotAllowedCommandException : CommandException {
        public SilenceNotAllowedCommandException() : base(@"silperr") { }
    }

    public class AlreadySilencedCommandException : CommandException {
        public AlreadySilencedCommandException() : base(@"silerr") { }
    }

    public class RevokeSilenceNotAllowedCommandException : CommandException {
        public RevokeSilenceNotAllowedCommandException() : base(@"usilperr") { }
    }

    public class NotSilencedCommandException : CommandException {
        public NotSilencedCommandException() : base(@"usilerr") { }
    }

    public class NickNameInUseCommandException : CommandException {
        public NickNameInUseCommandException(string nickName) : base(@"nameinuse", nickName) { }
    }

    public class UserListChannelNotFoundCommandException : CommandException {
        public UserListChannelNotFoundCommandException(string channelName) : base(@"whoerr", channelName) { }
    }

    public class InsufficientRankForChangeCommandException : CommandException {
        public InsufficientRankForChangeCommandException() : base(@"rankerr") { }
    }

    public class MessageNotFoundCommandException : CommandException {
        public MessageNotFoundCommandException() : base(@"delerr") { }
    }

    public class KickNotAllowedCommandException : CommandException {
        public KickNotAllowedCommandException(string userName) : base(@"kickna", userName) { }
    }

    public class NotBannedCommandException : CommandException {
        public NotBannedCommandException(string subject) : base(@"notban", subject) { }
    }

    public class ChannelNotFoundCommandException : CommandException {
        public ChannelNotFoundCommandException(string channelName) : base(@"nochan", channelName) { }
    }

    public class ChannelExistsCommandException : CommandException {
        public ChannelExistsCommandException(string channelName) : base(@"nischan", channelName) { }
    }

    public class ChannelRankCommandException : CommandException {
        public ChannelRankCommandException(string channelName) : base(@"ipchan", channelName) { }
        public ChannelRankCommandException(IChannel channel) : this(channel.Name) { }
    }

    public class ChannelPasswordCommandException : CommandException {
        public ChannelPasswordCommandException(string channelName) : base(@"ipwchan", channelName) { }
        public ChannelPasswordCommandException(IChannel channel) : this(channel.Name) { }
    }

    public class AlreadyInChannelCommandException : CommandException {
        public AlreadyInChannelCommandException(string channelName) : base(@"samechan", channelName) { }
        public AlreadyInChannelCommandException(IChannel channel) : this(channel.Name) { }
    }

    public class ChannelNameInvalidCommandException : CommandException {
        public ChannelNameInvalidCommandException() : base(@"inchan") { }
    }

    public class ChannelDeletionCommandException : CommandException {
        public ChannelDeletionCommandException(string channelName) : base(@"ndchan", channelName) { }
    }
}
