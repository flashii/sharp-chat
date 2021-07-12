using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Channels {
    public class ChannelException : Exception { }
    public class ChannelExistException : ChannelException { }
    public class ChannelInvalidNameException : ChannelException { }

    public class ChannelManager : IEventHandler {
        private Dictionary<string, Channel> Channels { get; } = new();

        private IConfig Config { get; }
        private CachedValue<string[]> ChannelIds { get; }

        private IEventDispatcher Dispatcher { get; }
        private ChatBot Bot { get; }
        private object Sync { get; } = new();

        public ChannelManager(IEventDispatcher dispatcher, IConfig config, ChatBot bot) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Bot = bot ?? throw new ArgumentNullException(nameof(bot));
            ChannelIds = Config.ReadCached(@"channels", new[] { @"lounge" });
        }

        public void UpdateChannels() {
            lock(Sync) {
                string[] channelIds = ChannelIds.Value.Clone() as string[];

                foreach(IChannel channel in Channels.Values) {
                    if(channelIds.Contains(channel.ChannelId)) {
                        using IConfig config = Config.ScopeTo($@"channels:{channel.ChannelId}");
                        string name = config.ReadValue(@"name", channel.ChannelId);
                        string topic = config.ReadValue(@"topic");
                        bool autoJoin = config.ReadValue(@"autoJoin", false);
                        string password = null;
                        int? minRank = null;
                        uint? maxCapacity = null;

                        if(!autoJoin) {
                            password = config.ReadValue(@"password", string.Empty);
                            if(string.IsNullOrEmpty(password))
                                password = null;

                            minRank = config.SafeReadValue(@"minRank", 0);
                            maxCapacity = config.SafeReadValue(@"maxCapacity", 0u);
                        }

                        Update(channel, name, topic, false, minRank, password, autoJoin, maxCapacity);
                    } else if(!channel.IsTemporary) // Not in config == temporary
                        Update(channel, temporary: true);
                }

                foreach(string channelId in channelIds) {
                    if(Channels.ContainsKey(channelId))
                        continue;
                    using IConfig config = Config.ScopeTo($@"channels:{channelId}");
                    string name = config.ReadValue(@"name", channelId);
                    string topic = config.ReadValue(@"topic");
                    bool autoJoin = config.ReadValue(@"autoJoin", false);
                    string password = null;
                    int minRank = 0;
                    uint maxCapacity = 0;

                    if(!autoJoin) {
                        password = config.ReadValue(@"password", string.Empty);
                        if(string.IsNullOrEmpty(password))
                            password = null;

                        minRank = config.SafeReadValue(@"minRank", 0);
                        maxCapacity = config.SafeReadValue(@"maxCapacity", 0u);
                    }

                    Create(channelId, Bot.UserId, name, topic, false, minRank, password, autoJoin, maxCapacity);
                }
            }
        }

        public void Remove(IChannel channel, IUser user = null) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));

            lock(Sync) {
                Channel chan = null;
                if(channel is Channel c && Channels.ContainsValue(c))
                    chan = c;
                else if(Channels.TryGetValue(channel.ChannelId, out Channel c2))
                    chan = c2;

                if(chan == null)
                    return; // exception?

                // Remove channel from the listing
                Channels.Remove(chan.ChannelId);

                // Broadcast death
                Dispatcher.DispatchEvent(this, new ChannelDeleteEvent(user ?? Bot, chan));

                // Move all users back to the main channel
                // TODO:!!!!!!!!! Replace this with a kick. SCv2 supports being in 0 channels, SCv1 should force the user back to DefaultChannel.
                // Could be handled by the user/session itself?
                //foreach(ChatUser user in channel.GetUsers()) {
                //    Context.SwitchChannel(user, DefaultChannel);
                //}

                // Broadcast deletion of channel (deprecated)
                /*foreach(IUser u in Users.OfRank(chan.MinimumRank))
                    u.SendPacket(new ChannelDeletePacket(chan));*/
            }
        }

        private bool Exists(string name) {
            if(name == null)
                throw new ArgumentNullException(nameof(name));
            lock(Sync)
                return Channels.Values.Any(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private void ValidateName(string name) {
            if(!name.All(c => char.IsLetter(c) || char.IsNumber(c) || c == '-'))
                throw new ChannelInvalidNameException();
            if(Exists(name))
                throw new ChannelExistException();
        }

        public IChannel Create(
            IUser user,
            string name,
            string topic = null,
            bool temp = true,
            int minRank = 0,
            string password = null,
            bool autoJoin = false,
            uint maxCapacity = 0
        ) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            return Create(user.UserId, name, topic, temp, minRank, password, autoJoin, maxCapacity);
        }

        public IChannel Create(
            long ownerId,
            string name,
            string topic = null,
            bool temp = true,
            int minRank = 0,
            string password = null,
            bool autoJoin = false,
            uint maxCapacity = 0
        ) => Create(RNG.NextString(Channel.ID_LENGTH), ownerId, name, topic, temp, minRank, password, autoJoin, maxCapacity);

        public IChannel Create(
            string channelId,
            long ownerId,
            string name,
            string topic = null,
            bool temp = true,
            int minRank = 0,
            string password = null,
            bool autoJoin = false,
            uint maxCapacity = 0,
            int order = 0
        ) {
            if(name == null)
                throw new ArgumentNullException(nameof(name));
            ValidateName(name);

            lock(Sync) {
                Channel channel = new(channelId, name, topic, temp, minRank, password, autoJoin, maxCapacity, ownerId, order);
                Channels.Add(channel.ChannelId, channel);

                Dispatcher.DispatchEvent(this, new ChannelCreateEvent(channel));

                // Broadcast creation of channel (deprecated)
                /*if(Users != null)
                    foreach(IUser user in Users.OfRank(channel.MinimumRank))
                        user.SendPacket(new ChannelCreatePacket(channel));*/

                return channel;
            }
        }

        public void Update(
            IChannel channel,
            string name = null,
            string topic = null,
            bool? temporary = null,
            int? minRank = null,
            string password = null,
            bool? autoJoin = null,
            uint? maxCapacity = null,
            int? order = null
        ) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));

            if(!(channel is Channel c && Channels.ContainsValue(c))) {
                if(Channels.TryGetValue(channel.ChannelId, out Channel c2))
                    channel = c2;
                else
                    throw new ArgumentException(@"Provided channel is not registered with this manager.", nameof(channel));
            }

            lock(Sync) {
                string prevName = channel.Name;
                bool nameUpdated = !string.IsNullOrWhiteSpace(name) && name != prevName;

                if(nameUpdated)
                    ValidateName(name);

                if(topic != null && channel.Topic.Equals(topic))
                    topic = null;

                if(temporary.HasValue && channel.IsTemporary == temporary.Value)
                    temporary = null;

                if(minRank.HasValue && channel.MinimumRank == minRank.Value)
                    minRank = null;

                if(password != null && channel.Password == password)
                    password = null;

                if(autoJoin.HasValue && channel.AutoJoin == autoJoin.Value)
                    autoJoin = null;

                if(maxCapacity.HasValue && channel.MaxCapacity == maxCapacity.Value)
                    maxCapacity = null;

                if(order.HasValue && channel.Order == order.Value)
                    order = null;

                Dispatcher.DispatchEvent(this, new ChannelUpdateEvent(channel, Bot, name, topic, temporary, minRank, password, autoJoin, maxCapacity, order));

                // Users that no longer have access to the channel/gained access to the channel by the hierarchy change should receive delete and create packets respectively
                // TODO: should be moved to the usermanager probably
                /*foreach(IUser user in Users.OfRank(channel.MinimumRank)) {
                    user.SendPacket(new ChannelUpdatePacket(prevName, channel));

                    if(nameUpdated)
                        user.ForceChannel();
                }*/
            }
        }

        public void GetChannel(Func<IChannel, bool> predicate, Action<IChannel> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Channels.Values.FirstOrDefault(predicate));
        }

        public void GetChannelById(string channelId, Action<IChannel> callback) {
            if(channelId == null)
                throw new ArgumentNullException(nameof(channelId));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(string.IsNullOrWhiteSpace(channelId)) {
                callback(null);
                return;
            }
            lock(Sync)
                callback(Channels.TryGetValue(channelId, out Channel channel) ? channel : null);
        }

        public void GetChannelByName(string name, Action<IChannel> callback) {
            if(name == null)
                throw new ArgumentNullException(nameof(name));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(string.IsNullOrWhiteSpace(name)) {
                callback(null);
                return;
            }
            GetChannel(c => name.Equals(c.Name, StringComparison.InvariantCultureIgnoreCase), callback);
        }

        public void GetChannel(IChannel channel, Action<IChannel> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                if(channel is Channel c && Channels.ContainsValue(c)) {
                    callback(c);
                    return;
                }

                GetChannel(channel.Equals, callback);
            }
        }

        public void GetChannels(Action<IEnumerable<IChannel>> callback, bool ordered = false) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                IEnumerable<IChannel> channels = Channels.Values;
                if(ordered)
                    channels = channels.OrderBy(c => c.Order);
                callback(channels);
            }
        }

        public void GetChannels(Func<IChannel, bool> predicate, Action<IEnumerable<IChannel>> callback, bool ordered = false) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                IEnumerable<IChannel> channels = Channels.Values.Where(predicate);
                if(ordered)
                    channels = channels.OrderBy(c => c.Order);
                callback(channels);
            }
        }

        public void GetDefaultChannels(Action<IEnumerable<IChannel>> callback, bool ordered = true) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            // it doesn't really make sense for a channel to be temporary and autojoin
            // maybe reconsider this in the future if the temp channel nuking strategy has adjusted
            GetChannels(c => c.AutoJoin && !c.IsTemporary, callback, ordered);
        }

        public void GetChannelsById(IEnumerable<string> channelIds, Action<IEnumerable<IChannel>> callback, bool ordered = false) {
            if(channelIds == null)
                throw new ArgumentNullException(nameof(channelIds));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c => channelIds.Contains(c.ChannelId), callback, ordered);
        }

        public void GetChannelsByName(IEnumerable<string> names, Action<IEnumerable<IChannel>> callback, bool ordered = false) {
            if(names == null)
                throw new ArgumentNullException(nameof(names));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c => names.Contains(c.Name), callback, ordered);
        }

        public void GetChannels(IEnumerable<IChannel> channels, Action<IEnumerable<IChannel>> callback, bool ordered = false) {
            if(channels == null)
                throw new ArgumentNullException(nameof(channels));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c1 => channels.Any(c2 => c2.Equals(c1)), callback, ordered);
        }

        public void GetChannels(int minRank, Action<IEnumerable<IChannel>> callback, bool ordered = false) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c => c.MinimumRank <= minRank, callback, ordered);
        }

        public void GetChannels(IUser user, Action<IEnumerable<IChannel>> callback, bool ordered = false) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c => c is Channel channel && channel.HasUser(user), callback, ordered);
        }

        public void VerifyPassword(IChannel channel, string password, Action<bool> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(password == null)
                throw new ArgumentNullException(nameof(password));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            GetChannel(channel, c => {
                if(c is not Channel channel) {
                    callback(false);
                    return;
                }
                
                if(!channel.HasPassword) {
                    callback(true);
                    return;
                }

                callback(channel.VerifyPassword(password));
            });
        }

        private void OnCreate(object sender, ChannelCreateEvent cce) {
            if(sender == this)
                return;

            lock(Sync) {
                if(Exists(cce.Name))
                    throw new ArgumentException(@"Channel already registered??????", nameof(cce));

                Channels.Add(cce.ChannelId, new Channel(
                    cce.ChannelId,
                    cce.Name,
                    cce.Topic,
                    cce.IsTemporary,
                    cce.MinimumRank,
                    cce.Password,
                    cce.AutoJoin,
                    cce.MaxCapacity,
                    cce.UserId,
                    cce.Order
                ));
            }
        }

        private void OnDelete(object sender, ChannelDeleteEvent cde) {
            if(sender == this)
                return;

            lock(Sync)
                Channels.Remove(cde.ChannelId);
        }

        private void OnEvent(object sender, IEvent evt) {
            Channel channel;
            lock(Sync)
                if(!Channels.TryGetValue(evt.ChannelId, out channel))
                    channel = null;
            channel?.HandleEvent(sender, evt);
        }

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case ChannelCreateEvent cce:
                    OnCreate(sender, cce);
                    break;
                case ChannelDeleteEvent cde:
                    OnDelete(sender, cde);
                    break;

                case ChannelUpdateEvent _:
                case ChannelUserJoinEvent _:
                case ChannelUserLeaveEvent _:
                    OnEvent(sender, evt);
                    break;
            }
        }
    }
}
