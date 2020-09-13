﻿using SharpChat.Events;
using SharpChat.Flashii;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SharpChat {
    public class ChatContext : IDisposable, IPacketTarget {
        public bool IsDisposed { get; private set; }

        public SockChatServer Server { get; }
        public Timer BumpTimer { get; }
        public BanManager Bans { get; }
        public ChannelManager Channels { get; }
        public UserManager Users { get; }
        public ChatEventManager Events { get; }

        public string TargetName => @"@broadcast";

        public ChatContext(SockChatServer server) {
            Server = server;
            Bans = new BanManager(this);
            Users = new UserManager(this);
            Channels = new ChannelManager(this);
            Events = new ChatEventManager(this);

            BumpTimer = new Timer(e => FlashiiBump.Submit(server.HttpClient, Users.WithActiveConnections()), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void Update() {
            Bans.RemoveExpired();
            CheckPings();
        }

        public void BanUser(ChatUser user, DateTimeOffset? until = null, bool banIPs = false, UserDisconnectReason reason = UserDisconnectReason.Kicked) {
            if (until.HasValue && until.Value <= DateTimeOffset.UtcNow)
                until = null;

            if (until.HasValue) {
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Banned, until.Value));
                Bans.Add(user, until.Value);

                if (banIPs) {
                    foreach (IPAddress ip in user.RemoteAddresses)
                        Bans.Add(ip, until.Value);
                }
            } else
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Kicked));

            user.Close();
            UserLeave(null, user, null, reason);
        }

        public void HandleJoin(ChatUser user, ChatChannel chan, ChatUserSession sess) {
            if (!chan.HasUser(user)) {
                chan.Send(new UserConnectPacket(DateTimeOffset.Now, user));
                Events.Add(new UserConnectEvent(DateTimeOffset.Now, user, chan));
            }

            sess.Send(new AuthSuccessPacket(user, chan, sess));
            sess.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IEnumerable<IChatEvent> msgs = Events.GetTargetLog(chan);

            foreach (IChatEvent msg in msgs)
                sess.Send(new ContextMessagePacket(msg));

            sess.Send(new ContextChannelsPacket(Channels.OfHierarchy(user.Rank)));

            if (!chan.HasUser(user))
                chan.UserJoin(user);

            sess.Channel = chan;

            if (!Users.Contains(user))
                Users.Add(user);
        }

        public void UserLeave(ChatChannel chan, ChatUser user, ChatUserSession sess, UserDisconnectReason reason = UserDisconnectReason.Leave) {
            user.Status = ChatUserStatus.Offline;

            if (chan == null) {
                foreach(ChatChannel channel in user.GetChannels())
                    UserLeave(channel, user, sess, reason);
                return;
            }

            if (chan.IsTemporary && chan.Owner == user)
                Channels.Remove(chan);

            chan.UserLeave(user);
            if(sess != null)
                sess.Channel = null;
            chan.Send(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
            Events.Add(new UserDisconnectEvent(DateTimeOffset.Now, user, chan, reason));
        }

        [Obsolete]
        public void SwitchChannel(ChatUser user, ChatChannel chan, string password) {
            foreach(ChatUserSession sess in user.GetSessions())
                SwitchChannel(user, chan, sess, password);
        }

        public void SwitchChannel(ChatUser user, ChatChannel chan, ChatUserSession sess, string password) {
            if (sess.Channel == chan) {
                //user.Send(true, @"samechan", chan.Name);
                sess.ForceChannel();
                return;
            }

            if (!user.Can(ChatUserPermissions.JoinAnyChannel) && chan.Owner != user) {
                if (chan.Rank > user.Rank) {
                    sess.Send(new LegacyCommandResponse(LCR.CHANNEL_INSUFFICIENT_HIERARCHY, true, chan.Name));
                    sess.ForceChannel();
                    return;
                }

                if (chan.Password != password) {
                    sess.Send(new LegacyCommandResponse(LCR.CHANNEL_INVALID_PASSWORD, true, chan.Name));
                    sess.ForceChannel();
                    return;
                }
            }

            ForceChannelSwitch(user, chan, sess);
        }

        public void ForceChannelSwitch(ChatUser user, ChatChannel chan, ChatUserSession sess) {
            if (!Channels.Contains(chan))
                return;

            ChatChannel oldChan = sess.Channel;

            oldChan.Send(new UserChannelLeavePacket(user));
            Events.Add(new UserChannelLeaveEvent(DateTimeOffset.Now, user, oldChan));
            chan.Send(new UserChannelJoinPacket(user));
            Events.Add(new UserChannelJoinEvent(DateTimeOffset.Now, user, chan));

            user.Send(new ContextClearPacket(chan, ContextClearMode.MessagesUsers));
            user.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IEnumerable<IChatEvent> msgs = Events.GetTargetLog(chan);

            foreach (IChatEvent msg in msgs)
                user.Send(new ContextMessagePacket(msg));

            oldChan.UserLeave(user);
            chan.UserJoin(user);
            sess.Channel = chan;
            sess.ForceChannel();

            if (oldChan.IsTemporary && oldChan.Owner == user)
                Channels.Remove(oldChan);
        }

        public void CheckPings() {
            lock(Users)
                foreach (ChatUser user in Users.All()) {
                    IEnumerable<ChatUserSession> timedOut = user.GetDeadSessions();

                    foreach(ChatUserSession sess in timedOut) {
                        user.RemoveSession(sess);
                        sess.Dispose();
                        Logger.Write($@"Nuked session {sess.Id} from {user.Username} (timeout)");
                    }

                    if(!user.HasSessions)
                        UserLeave(null, user, null, UserDisconnectReason.TimeOut);
                }
        }

        public void Send(IServerPacket packet) {
            foreach (ChatUser user in Users.All())
                user.Send(packet);
        }

        ~ChatContext()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            IsDisposed = true;

            BumpTimer?.Dispose();
            Events?.Dispose();
            Channels?.Dispose();
            Users?.Dispose();
            Bans?.Dispose();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
