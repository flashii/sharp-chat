using SharpChat.Messages;
using SharpChat.Protocol.SockChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class DeleteMessageCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"delmsg" || (name == @"delete" && args.ElementAtOrDefault(1)?.All(char.IsDigit) == true);

        private MessageManager Messages { get; }
        private IUser Sender { get; }

        public DeleteMessageCommand(MessageManager messages, IUser sender) {
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool DispatchCommand(CommandContext ctx) {
            bool deleteAnyMessage = ctx.User.Can(UserPermissions.DeleteAnyMessage);

            if(!deleteAnyMessage && !ctx.User.Can(UserPermissions.DeleteOwnMessage)) {
                ctx.Connection.SendPacket(new CommandNotAllowedErrorPacket(Sender, ctx.Args));
                return true;
            }

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long messageId)) {
                ctx.Connection.SendPacket(new CommandFormatErrorPacket(Sender));
                return true;
            }

            Messages.GetMessage(messageId, msg => {
                if(msg == null || msg.Sender.Rank > ctx.User.Rank
                    || (!deleteAnyMessage && msg.Sender.UserId != ctx.User.UserId))
                    ctx.Connection.SendPacket(new DeleteMessageNotFoundErrorPacket(Sender));
                else
                    Messages.Delete(ctx.User, msg);
            });

            return true;
        }
    }
}
