using System.Collections.Generic;

namespace SharpChat.Protocol.SockChat.Commands {
    public class WhisperCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name is @"whisper" or @"msg";

        public bool DispatchCommand(CommandContext ctx) {
            // reimplement this entirely
            // this should invoke the creation of a private temporary channel
            // if the client joins this channel, it should no longer use the Private message flag and just pump shit into that channel

            /*if(ctx.Args.Count() < 3)
                throw new CommandException(LCR.COMMAND_FORMAT_ERROR);

            string whisperUserName = ctx.Args.ElementAtOrDefault(1);
            ChatUser whisperUser = ctx.Chat.Users.Get(whisperUserName);

            if(whisperUser == null)
                throw new CommandException(LCR.USER_NOT_FOUND, whisperUserName);

            if(whisperUser == ctx.User)
                return null;

            string whisperStr = string.Join(' ', ctx.Args.Skip(2));

            whisperUser.Send(new ChatMessageAddPacket(new ChatMessageEvent(ctx.User, whisperUser, whisperStr, EventFlags.Private)));
            ctx.User.Send(new ChatMessageAddPacket(new ChatMessageEvent(ctx.User, ctx.User, $@"{whisperUser.DisplayName} {whisperStr}", EventFlags.Private)));*/
            return true;
        }
    }
}
