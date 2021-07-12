using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Users;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.IRC.Replies {
    public class NamesReply : Reply {
        public const int CODE = 353;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }
        private IEnumerable<string> UserNames { get; }

        public NamesReply(IChannel channel, IEnumerable<string> userNames) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            UserNames = userNames ?? throw new ArgumentNullException(nameof(userNames));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCNamesPrefix()} {Channel.GetIRCName()} :{string.Join(' ', UserNames)}";
        }

        public static void SendBatch(IRCConnection conn, IChannel channel, IEnumerable<IUser> users) {
            const int max_length = 400; // allow for 112 characters of overhead
            int length = 0;
            List<string> userNames = new();

            void sendBatch() {
                if(length < 1)
                    return;
                conn.SendReply(new NamesReply(channel, userNames));
                length = 0;
                userNames.Clear();
            };

            foreach(IUser user in users) {
                string name = user.GetIRCName(channel);
                int nameLength = name.Length + 1;

                if(length + nameLength > max_length)
                    sendBatch();

                length += nameLength;
                userNames.Add(name);
            }

            sendBatch();
        }
    }
}
