using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Users;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Protocol.IRC.ServerCommands {
    public class ServerPrivateMessageCommand : ServerCommand {
        public const string NAME = @"PRIVMSG";

        public override string CommandName => NAME;

        private IChannel Channel { get; }
        public override IUser Sender { get; }
        private string Line { get; }

        public ServerPrivateMessageCommand(IChannel channel, IUser sender, string line) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Line = line ?? throw new ArgumentNullException(nameof(line));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :{Line}";
        }

        public static IEnumerable<ServerPrivateMessageCommand> Split(IMessage message) {
            return Split(message.Channel, message.Sender, message.Text);
        }

        public static IEnumerable<ServerPrivateMessageCommand> Split(MessageCreateEvent mce) {
            return Split(mce.Channel, mce.User, mce.Text);
        }

        public static IEnumerable<ServerPrivateMessageCommand> Split(IChannel channel, IUser sender, string text) {
            Queue<string> parts = new Queue<string>(SplitText(text));
            while(parts.TryDequeue(out string part))
                yield return new ServerPrivateMessageCommand(channel, sender, part);
        }

        public static IEnumerable<string> SplitText(string text, int targetLength = 400) {
            Queue<string> lines = new Queue<string>(text.Split('\n'));

            static int getStartingChar(byte[] buff, int index) {
                return (buff[index] & 0xC0) == 0x80 ? getStartingChar(buff, index - 1) : index;
            };

            while(lines.TryDequeue(out string rawLine)) {
                byte[] line = Encoding.UTF8.GetBytes(rawLine.Trim());
                int chunks = line.Length / targetLength;
                int start = 0;

                for(int i = chunks; i > 0; --i) {
                    int end = getStartingChar(line, start + targetLength);
                    yield return Encoding.UTF8.GetString(line, start, end - start);
                    start = end;
                }

                if(line.Length > start)
                    yield return Encoding.UTF8.GetString(line, start, line.Length - start);
            }
        }
    }
}
