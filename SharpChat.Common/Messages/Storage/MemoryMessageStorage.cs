using SharpChat.Channels;
using SharpChat.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Messages.Storage {
    public class MemoryMessageStorage : IMessageStorage {
        private List<MemoryMessage> Messages { get; } = new List<MemoryMessage>();
        private List<MemoryMessageChannel> Channels { get; } = new List<MemoryMessageChannel>();
        private readonly object Sync = new object();

        public void GetMessage(long messageId, Action<IMessage> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Messages.FirstOrDefault(m => m.MessageId == messageId));
        }

        public void GetMessages(IChannel channel, Action<IEnumerable<IMessage>> callback, int amount, int offset) {
            lock(Sync) {
                IEnumerable<IMessage> subset = Messages.Where(m => m.Channel.Equals(channel));

                int start = subset.Count() - offset - amount;

                if(start < 0) {
                    amount += start;
                    start = 0;
                }

                callback(subset.Skip(start).Take(amount));
            }
        }

        private void StoreMessage(MessageCreateEvent mce) {
            lock(Sync) {
                MemoryMessageChannel channel = Channels.FirstOrDefault(c => c.Equals(mce.Channel));
                if(channel == null)
                    return; // This is basically an invalid state
                Messages.Add(new MemoryMessage(channel, mce));
            }
        }

        private void UpdateMessage(object sender, MessageUpdateEvent mue) {
            lock(Sync)
                Messages.FirstOrDefault(m => m.MessageId == mue.MessageId)?.HandleEvent(sender, mue);
        }

        private void DeleteMessage(MessageDeleteEvent mde) {
            lock(Sync)
                Messages.RemoveAll(m => m.MessageId == mde.MessageId);
        }

        private void CreateChannel(ChannelCreateEvent cce) {
            lock(Sync)
                Channels.Add(new MemoryMessageChannel(cce));
        }
        
        private void UpdateChannel(object sender, ChannelUpdateEvent cue) {
            lock(Sync)
                Channels.FirstOrDefault(c => c.Name.Equals(cue.PreviousName))?.HandleEvent(sender, cue);
        }

        private void DeleteChannel(ChannelDeleteEvent cde) {
            lock(Sync) {
                MemoryMessageChannel channel = Channels.FirstOrDefault(c => c.Equals(cde.Channel));
                if(channel == null)
                    return;
                Channels.Remove(channel);
                Messages.RemoveAll(m => m.Channel.Equals(channel));
            }
        }

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case MessageCreateEvent mce:
                    StoreMessage(mce);
                    break;
                case MessageUpdateEvent mue:
                    UpdateMessage(sender, mue);
                    break;
                case MessageDeleteEvent mde:
                    DeleteMessage(mde);
                    break;

                case ChannelCreateEvent cce:
                    CreateChannel(cce);
                    break;
                case ChannelUpdateEvent cue:
                    UpdateChannel(sender, cue);
                    break;
                case ChannelDeleteEvent cde:
                    DeleteChannel(cde);
                    break;
            }
        }
    }
}
