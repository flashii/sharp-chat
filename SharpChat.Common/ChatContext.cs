﻿using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Packets;
using SharpChat.RateLimiting;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using SharpChat.Messages.Storage;

namespace SharpChat {
    public class ChatContext : IDisposable, IEventDispatcher {
        public ChannelManager Channels { get; }
        public MessageManager Messages { get; }
        public UserManager Users { get; }
        public SessionManager Sessions { get; }
        public ChannelUserRelations ChannelUsers { get; }
        public RateLimiter RateLimiter { get; }

        public IDataProvider DataProvider { get; }

        public ChatBot Bot { get; } = new ChatBot(); 

        private Timer BumpTimer { get; }
        private readonly object Sync = new object();

        public ChatContext(Guid serverId, IConfig config, IDatabaseBackend databaseBackend, IDataProvider dataProvider) {
            if(config == null)
                throw new ArgumentNullException(nameof(config));

            DatabaseWrapper db = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));
            IMessageStorage msgStore = db.IsNullBackend
                ? new MemoryMessageStorage()
                : new ADOMessageStorage(db);

            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sessions = new SessionManager(serverId, config.ScopeTo(@"sessions"));
            Users = new UserManager(this);
            Channels = new ChannelManager(this, config, Bot);
            ChannelUsers = new ChannelUserRelations(this, Channels, Users, Sessions);
            Messages = new MessageManager(this, msgStore, config.ScopeTo(@"messages"));
            RateLimiter = new RateLimiter(config.ScopeTo(@"flood"));

            Channels.UpdateChannels();

            // Should probably not rely on Timers in the future
            BumpTimer = new Timer(e => {
                Logger.Write(@"Bumping last online times...");
                IEnumerable<IUser> users = null;
                Sessions.GetActiveSessions(s => users = s.Select(s => s.User));
                DataProvider.UserBumpClient.SubmitBumpUsers(Sessions, users);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void Update() { // this should probably not exist, or at least not called the way it is
            Sessions.DisposeTimedOut();
            PruneSessionlessUsers(); // this function also needs to go
        }

        // should this be moved to UserManager?
        public void BanUser(
            IUser user,
            TimeSpan duration,
            UserDisconnectReason reason = UserDisconnectReason.Kicked,
            bool isPermanent = false,
            IUser modUser = null,
            string textReason = null
        ) {
            ForceDisconnectPacket packet;

            if(duration != TimeSpan.Zero || isPermanent) {
                if(string.IsNullOrWhiteSpace(textReason))
                    textReason = reason switch {
                        UserDisconnectReason.Kicked => @"User was banned.",
                        UserDisconnectReason.Flood => @"User was kicked for flood protection.",
                        _ => @"Unknown reason given.",
                    };

                DataProvider.BanClient.CreateBan(user.UserId, modUser?.UserId ?? -1, isPermanent, duration, textReason);

                packet = new ForceDisconnectPacket(ForceDisconnectReason.Banned, duration, isPermanent);
            } else
                packet = new ForceDisconnectPacket(ForceDisconnectReason.Kicked);

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

        [Obsolete(@"Refactor to use ChannelUsers.LeaveChannel and Users.Disconnect")]
        public void UserLeave(IChannel chan, IUser user, UserDisconnectReason reason = UserDisconnectReason.Leave) {
            // handle in channelusers
            //chan.SendPacket(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
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

        [Obsolete(@"Use ChannelUsers.LeaveChannel")]
        public void LeaveChannel(IUser user, IChannel channel) {
            // handle in channelusers
            //channel.SendPacket(new UserChannelLeavePacket(user));
        }

        public void PruneSessionlessUsers() {
            Users.GetUsers(users => {
                foreach(IUser user in users)
                    if(Sessions.GetSessionCount(user) < 1)
                        Users.Disconnect(user, UserDisconnectReason.TimeOut);
            });
        }

        public void SendPacket(IServerPacket packet) {
            Sessions.SendPacket(packet);
        }

        public void DispatchEvent(object sender, IEvent evt) {
            if(evt == null)
                throw new ArgumentNullException(nameof(evt));

            lock(Sync) {
                Logger.Debug(evt);

                Sessions.HandleEvent(sender, evt);
                ChannelUsers.HandleEvent(sender, evt);
                Channels.HandleEvent(sender, evt);
                Users.HandleEvent(sender, evt);
                Messages.HandleEvent(sender, evt);
            }
        }

        private bool IsDisposed;
        ~ChatContext()
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
