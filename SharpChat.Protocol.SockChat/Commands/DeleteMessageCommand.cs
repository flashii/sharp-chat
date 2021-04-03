using SharpChat.Messages;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Protocol.SockChat.Commands {
    public class DeleteMessageCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"delmsg" || (name == @"delete" && args.ElementAtOrDefault(1)?.All(char.IsDigit) == true);

        private MessageManager Messages { get; }

        public DeleteMessageCommand(MessageManager messages) {
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public bool DispatchCommand(CommandContext ctx) {
            bool deleteAnyMessage = ctx.User.Can(UserPermissions.DeleteAnyMessage);

            if(!deleteAnyMessage && !ctx.User.Can(UserPermissions.DeleteOwnMessage))
                throw new CommandNotAllowedException(ctx.Args);

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long messageId))
                throw new CommandFormatException();

            IMessage delMsg = Messages.GetMessage(messageId);

            if(delMsg == null || delMsg.Sender.Rank > ctx.User.Rank
                || (!deleteAnyMessage && delMsg.Sender.UserId != ctx.User.UserId))
                throw new MessageNotFoundCommandException();

            Messages.Delete(ctx.User, delMsg);
            return true;
        }
    }
}
