using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Messages.Storage;
using SharpChat.RateLimiting;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SharpChat {
    public class Context : IDisposable, IEventDispatcher {
        public const int ID_LENGTH = 8;
        public string ServerId { get; }

        public SessionManager Sessions { get; }
        public UserManager Users { get; }
        public ChannelManager Channels { get; }
        public ChannelUserRelations ChannelUsers { get; }
        public MessageManager Messages { get; }

        public IDataProvider DataProvider { get; }
        public RateLimiter RateLimiter { get; }

        public WelcomeMessage WelcomeMessage { get; }

        public ChatBot Bot { get; } = new(); 

        private Timer BumpTimer { get; }
        private readonly object Sync = new();

        private List<IEventHandler> EventHandlers { get; } = new();

        public DateTimeOffset Created { get; }

        public Context(IConfig config, IDatabaseBackend databaseBackend, IDataProvider dataProvider) {
            if(config == null)
                throw new ArgumentNullException(nameof(config));

            ServerId = RNG.NextString(ID_LENGTH); // maybe read this from the cfg instead
            Created = DateTimeOffset.Now; // read this from config definitely

            DatabaseWrapper db = new(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));
            IMessageStorage msgStore = db.IsNullBackend
                ? new MemoryMessageStorage()
                : new ADOMessageStorage(db);

            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Users = new UserManager(this);
            Sessions = new SessionManager(this, Users, config.ScopeTo(@"sessions"), ServerId);
            Messages = new MessageManager(this, msgStore, config.ScopeTo(@"messages"));
            Channels = new ChannelManager(this, config, Bot);
            ChannelUsers = new ChannelUserRelations(this, Channels, Users, Sessions, Messages);
            RateLimiter = new RateLimiter(config.ScopeTo(@"flood"));

            WelcomeMessage = new WelcomeMessage(config.ScopeTo(@"welcome"));

            AddEventHandler(Sessions);
            AddEventHandler(Users);
            AddEventHandler(Channels);
            AddEventHandler(ChannelUsers);
            AddEventHandler(Messages);

            Channels.UpdateChannels();

            // Should probably not rely on Timers in the future
            BumpTimer = new Timer(e => {
                Logger.Write(@"Nuking dead sessions and bumping online times...");
                Sessions.CheckTimeOut();
                IEnumerable<IUser> users = null;
                Sessions.GetActiveSessions(s => users = s.Select(s => s.User));
                DataProvider.UserBumpClient.SubmitBumpUsers(Sessions, users);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void BroadcastMessage(string text) {
            DispatchEvent(this, new BroadcastMessageEvent(Bot, text));
        }

        // should this be moved to UserManager?
        [Obsolete(@"should be moved elsewhere, UserManager probably")]
        public void BanUser(
            IUser user,
            TimeSpan duration,
            UserDisconnectReason reason = UserDisconnectReason.Kicked,
            bool isPermanent = false,
            IUser modUser = null,
            string textReason = null
        ) {
            //ForceDisconnectPacket packet;

            if(duration != TimeSpan.Zero || isPermanent) {
                if(string.IsNullOrWhiteSpace(textReason))
                    textReason = reason switch {
                        UserDisconnectReason.Kicked => @"User was banned.",
                        UserDisconnectReason.Flood => @"User was kicked for flood protection.",
                        _ => @"Unknown reason given.",
                    };

                DataProvider.BanClient.CreateBan(user.UserId, modUser?.UserId ?? -1, isPermanent, duration, textReason);

                //packet = new ForceDisconnectPacket(ForceDisconnectReason.Banned, duration, isPermanent);
            }// else
             //   packet = new ForceDisconnectPacket(ForceDisconnectReason.Kicked);

            // handle in users
            //user.SendPacket(packet);

            // channel users
            ChannelUsers.GetChannels(user, c => {
                foreach(IChannel chan in c)
                    ChannelUsers.LeaveChannel(chan, user, reason);
            });

            // a disconnect with Flood, Kicked or Banned should probably cause this
            // maybe forced disconnects should be their own event?
            Users.Disconnect(user, reason);
        }

        [Obsolete(@"Use ChannelUsers.JoinChannel")]
        public void JoinChannel(IUser user, IChannel channel) {
            // handle in channelusers
            //channel.SendPacket(new UserChannelJoinPacket(user));

            // send after join packet for v1
            //user.SendPacket(new ContextClearPacket(channel, ContextClearMode.MessagesUsers));

            // send after join
            //ChannelUsers.GetUsers(channel, u => user.SendPacket(new ContextUsersPacket(u.Except(new[] { user }).OrderByDescending(u => u.Rank))));

            // send after join, maybe add a capability that makes this implicit?
            /*Messages.GetMessages(channel, m => {
                foreach(IMessage msg in m)
                    user.SendPacket(new ContextMessagePacket(msg));
            });*/

            // should happen implicitly for v1 clients
            //user.ForceChannel(channel);
        }

        public void AddEventHandler(IEventHandler handler) {
            if(handler == null)
                throw new ArgumentNullException(nameof(handler));
            lock(Sync)
                if(!EventHandlers.Contains(handler))
                    EventHandlers.Add(handler);
        }

        public void RemoveEventHandler(IEventHandler handler) {
            if(handler == null)
                throw new ArgumentNullException(nameof(handler));
            // prevent asshattery
            if(handler == Sessions
                || handler == Users
                || handler == Channels
                || handler == ChannelUsers
                || handler == Messages)
                return;
            lock(Sync)
                EventHandlers.Remove(handler);
        }

        // DispatchEvent is responsible for cool deadlocks
        // A proper queue should be implemented and DispatchEvent should become non-blocking
        // Most uses of it should be fine with this but I think there's a couple instances where blocking is assumed
        // Retrieval functions in the managers should also become assumedly non-blocking to provide the possibility of pulling shit from DB/DP
        public void DispatchEvent(object sender, IEvent evt) {
            if(evt == null)
                throw new ArgumentNullException(nameof(evt));

            lock(Sync) {
                Logger.Debug(evt);

                foreach(IEventHandler handler in EventHandlers)
                    handler.HandleEvent(sender, evt);
            }
        }

        private bool IsDisposed;
        ~Context()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if (IsDisposed)
                return;
            IsDisposed = true;

            BumpTimer.Dispose();
        }
    }
}
