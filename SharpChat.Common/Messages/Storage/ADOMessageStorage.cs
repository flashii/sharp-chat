using SharpChat.Channels;
using SharpChat.Database;
using SharpChat.Events;
using System;
using System.Collections.Generic;

namespace SharpChat.Messages.Storage {
    public partial class ADOMessageStorage : IMessageStorage {
        private DatabaseWrapper Wrapper { get; }

        public ADOMessageStorage(DatabaseWrapper wrapper) {
            Wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
            RunMigrations();
        }

        public void GetMessage(long messageId, Action<IMessage> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            IMessage msg = null;

            Wrapper.RunQuery(
                @"SELECT `msg_id`, `msg_channel_name`, `msg_sender_id`, `msg_sender_name`, `msg_sender_colour`, `msg_sender_rank`, `msg_sender_nick`"
                + @", `msg_sender_perms`, `msg_text`, `msg_flags`"
                + @", " + Wrapper.ToUnixTime(@"`msg_created`") + @" AS `msg_created`"
                + @", " + Wrapper.ToUnixTime(@"`msg_edited`") + @" AS `msg_edited`"
                + @" FROM `sqc_messages`"
                + @" WHERE `msg_id` = @id"
                + @" AND `msg_deleted` IS NULL"
                + @" LIMIT 1",
                reader => {
                    if(reader.Next())
                        msg = new ADOMessage(reader);
                },
                Wrapper.CreateParam(@"id", messageId)
            );

            callback(msg);
        }

        public void GetMessages(IChannel channel, Action<IEnumerable<IMessage>> callback, int amount, int offset) {
            List<IMessage> msgs = new List<IMessage>();

            Wrapper.RunQuery(
                @"SELECT `msg_id`, `msg_channel_name`, `msg_sender_id`, `msg_sender_name`, `msg_sender_colour`, `msg_sender_rank`, `msg_sender_nick`"
                + @", `msg_sender_perms`, `msg_text`, `msg_flags`"
                + @", " + Wrapper.ToUnixTime(@"`msg_created`") + @" AS `msg_created`"
                + @", " + Wrapper.ToUnixTime(@"`msg_edited`") + @" AS `msg_edited`"
                + @" FROM `sqc_messages`"
                + @" WHERE `msg_channel_name` = @channelName"
                + @" AND `msg_deleted` IS NULL"
                + @" ORDER BY `msg_id` DESC"
                + @" LIMIT @amount OFFSET @offset",
                reader => {
                    while(reader.Next())
                        msgs.Add(new ADOMessage(reader));
                },
                Wrapper.CreateParam(@"channelName", channel.Name),
                Wrapper.CreateParam(@"amount", amount),
                Wrapper.CreateParam(@"offset", offset)
            );

            msgs.Reverse();
            callback(msgs);
        }

        private void StoreMessage(MessageCreateEvent mce) {
            byte flags = 0;
            if(mce.IsAction)
                flags |= ADOMessage.IS_ACTION;

            Wrapper.RunCommand(
                @"INSERT INTO `sqc_messages` ("
                    + @"`msg_id`, `msg_channel_name`, `msg_sender_id`, `msg_sender_name`, `msg_sender_colour`, `msg_sender_rank`"
                    + @", `msg_sender_nick`, `msg_sender_perms`, `msg_text`, `msg_flags`, `msg_created`"
                + @") VALUES ("
                    + @"@id, @channelName, @senderId, @senderName, @senderColour, @senderRank, @senderNick, @senderPerms"
                    + @", @text, @flags, " + Wrapper.FromUnixTime(@"@created")
                + @");",
                Wrapper.CreateParam(@"id", mce.MessageId),
                Wrapper.CreateParam(@"channelName", mce.Channel.Name),
                Wrapper.CreateParam(@"senderId", mce.User.UserId),
                Wrapper.CreateParam(@"senderName", mce.User.UserName),
                Wrapper.CreateParam(@"senderColour", mce.User.Colour.Raw),
                Wrapper.CreateParam(@"senderRank", mce.User.Rank),
                Wrapper.CreateParam(@"senderNick", string.IsNullOrWhiteSpace(mce.User.NickName) ? null : mce.User.NickName),
                Wrapper.CreateParam(@"senderPerms", mce.User.Permissions),
                Wrapper.CreateParam(@"text", mce.Text),
                Wrapper.CreateParam(@"flags", flags),
                Wrapper.CreateParam(@"created", mce.DateTime.ToUnixTimeSeconds())
            );
        }

        private void UpdateMessage(MessageUpdateEvent mue) {
            Wrapper.RunCommand(
                @"UPDATE `sqc_messages` SET `msg_text` = @text, `msg_edited` = " + Wrapper.FromUnixTime(@"@edited") + @" WHERE `msg_id` = @id",
                Wrapper.CreateParam(@"text", mue.Text),
                Wrapper.CreateParam(@"edited", mue.DateTime.ToUnixTimeSeconds()),
                Wrapper.CreateParam(@"id", mue.MessageId)
            );
        }

        private void DeleteMessage(MessageDeleteEvent mde) {
            Wrapper.RunCommand(
                @"UPDATE `sqc_messages` SET `msg_deleted` = " + Wrapper.FromUnixTime(@"@deleted") + @" WHERE `msg_id` = @id",
                Wrapper.CreateParam(@"deleted", mde.DateTime.ToUnixTimeSeconds()),
                Wrapper.CreateParam(@"id", mde.MessageId)
            );
        }

        private void UpdateChannel(ChannelUpdateEvent cue) {
            if(!cue.HasName)
                return;

            Wrapper.RunCommand(
                @"UPDATE `sqc_messages` SET `msg_channel_name` = @newName WHERE `msg_channel_name` = @oldName",
                Wrapper.CreateParam(@"newName", cue.Name),
                Wrapper.CreateParam(@"oldName", cue.PreviousName)
            );
        }

        private void DeleteChannel(ChannelDeleteEvent cde) {
            Wrapper.RunCommand(
                @"UPDATE `sqc_messages` SET `msg_deleted` = " + Wrapper.FromUnixTime(@"@deleted") + @" WHERE `msg_channel_name` = @name AND `msg_deleted` IS NULL",
                Wrapper.CreateParam(@"deleted", cde.DateTime.ToUnixTimeSeconds()),
                Wrapper.CreateParam(@"name", cde.Channel.Name)
            );
        }

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case MessageCreateEvent mce:
                    StoreMessage(mce);
                    break;
                case MessageUpdateEvent mue:
                    UpdateMessage(mue);
                    break;
                case MessageDeleteEvent mde:
                    DeleteMessage(mde);
                    break;

                case ChannelUpdateEvent cue:
                    UpdateChannel(cue);
                    break;
                case ChannelDeleteEvent cde:
                    DeleteChannel(cde);
                    break;
            }
        }
    }
}
