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
        private List<Channel> Channels { get; } = new List<Channel>();

        private IConfig Config { get; }
        private CachedValue<string[]> ChannelNames { get; }

        private IEventDispatcher Dispatcher { get; }
        private ChatBot Bot { get; }
        private object Sync { get; } = new object();

        public ChannelManager(IEventDispatcher dispatcher, IConfig config, ChatBot bot) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Bot = bot ?? throw new ArgumentNullException(nameof(bot));
            ChannelNames = Config.ReadCached(@"channels", new[] { @"lounge" });
        }

        public IChannel DefaultChannel { get; private set; } // does there need to be a more explicit way of assigning this?

        public void UpdateChannels() {
            lock(Sync) {
                string[] channelNames = ChannelNames;

                foreach(IChannel channel in Channels) {
                    if(channelNames.Contains(channel.Name)) {
                        using IConfig config = Config.ScopeTo($@"channels:{channel.Name}");
                        string topic = config.ReadValue(@"topic");
                        bool autoJoin = config.ReadValue(@"autoJoin", DefaultChannel == null || DefaultChannel == channel);
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

                        Update(channel, null, topic, false, minRank, password, autoJoin, maxCapacity);
                    } else if(!channel.IsTemporary) // Not in config == temporary
                        Update(channel, temporary: true);
                }

                foreach(string channelName in channelNames) {
                    if(Channels.Any(x => x.Name == channelName))
                        continue;
                    using IConfig config = Config.ScopeTo($@"channels:{channelName}");
                    string topic = config.ReadValue(@"topic");
                    bool autoJoin = config.ReadValue(@"autoJoin", DefaultChannel == null || DefaultChannel.Name == channelName);
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

                    Create(Bot, channelName, topic, false, minRank, password, autoJoin, maxCapacity);
                }

                if(DefaultChannel == null || DefaultChannel.IsTemporary || !channelNames.Contains(DefaultChannel.Name))
                    DefaultChannel = Channels.FirstOrDefault(c => !c.IsTemporary && c.AutoJoin);
            }
        }

        public void Remove(IChannel channel, IUser user = null) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(channel == DefaultChannel)
                return; // exception?

            lock(Sync) {
                Channel chan;
                if(channel is Channel c && Channels.Contains(c))
                    chan = c;
                else
                    chan = Channels.FirstOrDefault(c => c.Equals(channel));

                if(chan == null)
                    return; // exception?

                // Remove channel from the listing
                Channels.Remove(chan);

                // Broadcast death
                Dispatcher.DispatchEvent(this, new ChannelDeleteEvent(chan, user ?? Bot));

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
                return Channels.Any(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private void ValidateName(string name) {
            if(!name.All(c => char.IsLetter(c) || char.IsNumber(c) || c == '-'))
                throw new ChannelInvalidNameException();
            if(Exists(name))
                throw new ChannelExistException();
        }

        public IChannel Create(
            IUser owner,
            string name,
            string topic = null,
            bool temp = true,
            int minRank = 0,
            string password = null,
            bool autoJoin = false,
            uint maxCapacity = 0
        ) {
            if(name == null)
                throw new ArgumentNullException(nameof(name));
            ValidateName(name);

            lock(Sync) {
                Channel channel = new Channel(name, topic, temp, minRank, password, autoJoin, maxCapacity, owner);
                Channels.Add(channel);
                
                // Should this remain?
                if(DefaultChannel == null)
                    DefaultChannel = channel;
                
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
            uint? maxCapacity = null
        ) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(!Channels.Contains(channel))
                throw new ArgumentException(@"Provided channel is not registered with this manager.", nameof(channel));

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

                Dispatcher.DispatchEvent(this, new ChannelUpdateEvent(channel, Bot, name, topic, temporary, minRank, password, autoJoin, maxCapacity));

                // Users that no longer have access to the channel/gained access to the channel by the hierarchy change should receive delete and create packets respectively
                // TODO: should be moved to the usermanager probably
                /*foreach(IUser user in Users.OfRank(channel.MinimumRank)) {
                    user.SendPacket(new ChannelUpdatePacket(prevName, channel));

                    if(nameUpdated)
                        user.ForceChannel();
                }*/
            }
        }

        [Obsolete(@"Use void GetChannel(IChannel channel, Action<IChannel> callback)")]
        public IChannel GetChannel(IChannel channel) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            lock(Sync) {
                if(channel is Channel c && Channels.Contains(c))
                    return c;
                return Channels.FirstOrDefault(c => c.Equals(channel));
            }
        }

        public void GetChannel(Func<IChannel, bool> predicate, Action<IChannel> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Channels.FirstOrDefault(predicate));
        }

        public void GetChannel(string name, Action<IChannel> callback) {
            if(name == null)
                throw new ArgumentNullException(nameof(name));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannel(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase), callback);
        }

        public void GetChannel(IChannel channel, Action<IChannel> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync) {
                if(channel is Channel c && Channels.Contains(c)) {
                    callback(c);
                    return;
                }

                GetChannel(channel.Equals, callback);
            }
        }

        public void GetChannels(Action<IEnumerable<IChannel>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Channels);
        }

        public void GetChannels(Func<IChannel, bool> predicate, Action<IEnumerable<IChannel>> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Channels.Where(predicate));
        }

        public void GetChannels(IEnumerable<string> names, Action<IEnumerable<IChannel>> callback) {
            if(names == null)
                throw new ArgumentNullException(nameof(names));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c => names.Contains(c.Name), callback);
        }

        public void GetChannels(IEnumerable<IChannel> channels, Action<IEnumerable<IChannel>> callback) {
            if(channels == null)
                throw new ArgumentNullException(nameof(channels));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c1 => channels.Any(c2 => c2.Equals(c1)), callback);
        }

        public void GetChannels(int minRank, Action<IEnumerable<IChannel>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c => c.MinimumRank <= minRank, callback);
        }

        public void GetChannels(IUser user, Action<IEnumerable<IChannel>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(c => c is Channel channel && channel.HasUser(user), callback);
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

                Channels.Add(new Channel(
                    cce.Name,
                    cce.Topic,
                    cce.IsTemporary,
                    cce.MinimumRank,
                    cce.Password,
                    cce.AutoJoin,
                    cce.MaxCapacity,
                    cce.User
                ));
            }
        }

        private void OnDelete(object sender, ChannelDeleteEvent cde) {
            if(sender == this)
                return;

            lock(Sync) {
                Channel channel = Channels.FirstOrDefault(c => c.Equals(cde.Channel));
                if(channel != null)
                    Channels.Remove(channel);
            }
        }

        private void OnEvent(object sender, IEvent evt) {
            lock(Sync) {
                Channel channel = Channels.FirstOrDefault(c => c.Equals(evt.Channel));
                if(channel != null)
                    channel.HandleEvent(sender, evt);
            }
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
