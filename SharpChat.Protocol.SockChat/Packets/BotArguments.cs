using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class BotArguments {
        public const char SEPARATOR = '\f';

        public const string BANS = @"banlist"; // [ban list html]
        public const string BROADCAST = @"say"; // [broadcast message]
        public const string CHANNEL_CREATED = @"crchan"; // [channel name]
        public const string CHANNEL_DELETED = @"delchan"; // [target channel name]
        public const string CHANNEL_PASSWORD_CHANGED = @"cpwdchan"; // []
        public const string CHANNEL_RANK_CHANGED = @"cprivchan"; // []
        public const string FLOOD_WARNING = @"flwarn"; // []
        public const string BAN_PARDON = @"unban"; // [target user name or ip address]
        public const string SILENCE_PLACE_NOTICE = @"silence"; // []
        public const string SILENCE_PLACE_CONFIRM = @"silok"; // [target user name]
        public const string SILENCE_REVOKE_NOTICE = @"unsil"; // []
        public const string SILENCE_REVOKE_CONFIRM = @"usilok"; // [target user name]
        public const string USER_LIST_ALL = @"who"; // [user list html]
        public const string USER_LIST_CHANNEL = @"whochan"; // [target channel name, user list html]
        public const string NICKNAME_CHANGE = @"nick"; // [old display name, new display name]
        public const string USER_IP_ADDRESS = @"ipaddr"; // [target user name, user ip address]
        public const string WELCOME = @"welcome"; // [welcome message]

        public const string GENERIC_ERROR = @"generr"; // []
        public const string COMMAND_NOT_FOUND = @"nocmd"; // [target channel name]
        public const string COMMAND_NOT_ALLOWED = @"cmdna"; // [/target command name]
        public const string COMMAND_FORMAT_ERROR = @"cmderr"; // []
        public const string USER_NOT_FOUND_ERROR = @"usernf"; // [target user name]
        public const string SILENCE_SELF_ERROR = @"silself"; // []
        public const string SILENCE_NOT_ALLOWED_ERROR = @"silperr"; // []
        public const string SILENCE_ALREADY_ERROR = @"silerr"; // []
        public const string SILENCE_REVOKE_NOT_ALLOWED_ERROR = @"usilperr"; // []
        public const string SILENCE_REVOKE_ALREADY_ERROR = @"usilerr"; // []
        public const string NICKNAME_IN_USE_ERROR = @"nameinuse"; // [target nick name]
        public const string USER_LIST_ERROR = @"whoerr"; // [target channel name]
        public const string INSUFFICIENT_RANK_ERROR = @"rankerr"; // []
        public const string DELETE_MESSAGE_NOT_FOUND_ERROR = @"delerr"; // []
        public const string KICK_NOT_ALLOWED_ERROR = @"kickna"; // []
        public const string NOT_BANNED_ERROR = @"notban"; // [target user name]
        public const string CHANNEL_NOT_FOUND_ERROR = @"nochan"; // [target user name]
        public const string CHANNEL_EXISTS_ERROR = @"nischan"; // [desired channel name]
        public const string CHANNEL_INSUFFICIENT_RANK_ERROR = @"ipchan"; // [target channel name]
        public const string CHANNEL_INVALID_PASSWORD_ERROR = @"ipwchan"; // [target channel name]
        public const string CHANNEL_ALREADY_JOINED_ERROR = @"samechan"; // [target channel name]
        public const string CHANNEL_NAME_FORMAT_ERROR = @"inchan"; // []
        public const string CHANNEL_DELETE_ERROR = @"ndchan"; // [target channel name]

        public bool IsError { get; }
        public string Name { get; }
        public IEnumerable<object> Arguments { get; }

        public BotArguments(string name, bool error, params object[] args) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsError = error;
            Arguments = args;
        }

        public override string ToString() {
            StringBuilder sb = new();
            sb.Append(IsError ? '1' : '0');
            sb.Append(SEPARATOR);
            sb.Append(Name);

            foreach(object arg in Arguments) {
                sb.Append(SEPARATOR);
                sb.Append(arg);
            }

            return sb.ToString();
        }
    }
}
