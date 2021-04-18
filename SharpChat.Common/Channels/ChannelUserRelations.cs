using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Channels {
    public class ChannelUserRelations : IEventHandler {
        private IEventDispatcher Dispatcher { get; }
        private ChannelManager Channels { get; }
        private UserManager Users { get; }
        private SessionManager Sessions { get; }
        private MessageManager Messages { get; }

        public ChannelUserRelations(
            IEventDispatcher dispatcher,
            ChannelManager channels,
            UserManager users,
            SessionManager sessions,
            MessageManager messages
        ) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public void HasUser(IChannel channel, IUser user, Action<bool> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            Channels.GetChannel(channel, c => {
                if(c is not Channel channel) {
                    callback(false);
                    return;
                }

                callback(channel.HasUser(user));
            });
        }

        public void HasSession(IChannel channel, ISession session, Action<bool> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            Channels.GetChannel(channel, c => {
                if(c is not Channel channel) {
                    callback(false);
                    return;
                }

                callback(channel.HasSession(session));
            });
        }

        public void CountUsers(IChannel channel, Action<int> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            Channels.GetChannel(channel, c => {
                if(c is not Channel channel) {
                    callback(-1);
                    return;
                }

                callback(channel.CountUsers());
            });
        }

        public void CountUserSessions(IChannel channel, IUser user, Action<int> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            Channels.GetChannel(channel, c => {
                if(c is not Channel channel) {
                    callback(-1);
                    return;
                }

                callback(channel.CountUserSessions(user));
            });
        }

        public void CheckOverCapacity(IChannel channel, IUser user, Action<bool> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            Channels.GetChannel(channel, channel => {
                if(channel == null) {
                    callback(true);
                    return;
                }

                if(!channel.HasMaxCapacity() || user.UserId == channel.OwnerId) {
                    callback(false);
                    return;
                }

                CountUsers(channel, userCount => callback(channel == null || userCount >= channel.MaxCapacity));
            });
        }

        public void GetUsers(string channelName, Action<IEnumerable<IUser>> callback) {
            if(channelName == null)
                throw new ArgumentNullException(nameof(channelName));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(string.IsNullOrWhiteSpace(channelName)) {
                callback(Enumerable.Empty<IUser>());
                return;
            }
            Channels.GetChannel(channelName, c => GetUsersWithChannelCallback(c, callback));
        }

        public void GetUsers(IChannel channel, Action<IEnumerable<IUser>> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            Channels.GetChannel(channel, c => GetUsersWithChannelCallback(c, callback));
        }

        private void GetUsersWithChannelCallback(IChannel c, Action<IEnumerable<IUser>> callback) {
            if(c is not Channel channel) {
                callback(Enumerable.Empty<IUser>());
                return;
            }

            channel.GetUserIds(ids => Users.GetUsers(ids, callback));
        }

        public void GetUsers(IEnumerable<IChannel> channels, Action<IEnumerable<IUser>> callback) {
            if(channels == null)
                throw new ArgumentNullException(nameof(channels));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            // this is pretty disgusting
            Channels.GetChannels(channels, channels => {
                HashSet<long> ids = new HashSet<long>();

                foreach(IChannel c in channels) {
                    if(c is not Channel channel)
                        continue;

                    channel.GetUserIds(u => {
                        foreach(long id in u)
                            ids.Add(id);
                    });
                }

                Users.GetUsers(ids, callback);
            });
        }

        // this makes me cry
        public void GetUsers(IUser user, Action<IEnumerable<IUser>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            HashSet<IUser> all = new HashSet<IUser>();

            Channels.GetChannels(channels => {
                foreach(IChannel channel in channels) {
                    GetUsers(channel, users => {
                        foreach(IUser user in users)
                            all.Add(user);
                    });
                }
            });

            callback(all);
        }

        public void GetLocalSessionsByChannelName(string channelName, Action<IEnumerable<ISession>> callback) {
            if(channelName == null)
                throw new ArgumentNullException(nameof(channelName));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(string.IsNullOrWhiteSpace(channelName)) {
                callback(Enumerable.Empty<ISession>());
                return;
            }
            Channels.GetChannel(channelName, c => GetLocalSessionsChannelCallback(c, callback));
        }

        public void GetLocalSessions(IChannel channel, Action<IEnumerable<ISession>> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            Channels.GetChannel(channel, c => GetLocalSessionsChannelCallback(c, callback));
        }

        private void GetLocalSessionsChannelCallback(IChannel c, Action<IEnumerable<ISession>> callback) {
            if(c is not Channel channel) {
                callback(Enumerable.Empty<ISession>());
                return;
            }

            channel.GetSessionIds(ids => Sessions.GetLocalSessions(ids, callback));
        }

        public void GetLocalSessions(IUser user, Action<IEnumerable<ISession>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetChannels(user, channels => GetLocalSessionsUserCallback(channels, callback));
        }

        public void GetLocalSessionsByUserId(long userId, Action<IEnumerable<ISession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(userId < 1) {
                callback(Enumerable.Empty<ISession>());
                return;
            }
            GetChannelsByUserId(userId, channels => GetLocalSessionsUserCallback(channels, callback));
        }

        private void GetLocalSessionsUserCallback(IEnumerable<IChannel> channels, Action<IEnumerable<ISession>> callback) {
            if(!channels.Any()) {
                callback(Enumerable.Empty<ISession>());
                return;
            }

            Channels.GetChannels(channels, channels => {
                HashSet<string> sessionIds = new HashSet<string>();

                foreach(IChannel c in channels) {
                    if(c is not Channel channel)
                        continue;
                    channel.GetSessionIds(ids => {
                        foreach(string id in ids)
                            sessionIds.Add(id);
                    });
                }

                Sessions.GetLocalSessions(sessionIds, callback);
            });
        }

        public void GetChannelsByUserId(long userId, Action<IEnumerable<IChannel>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(userId < 1) {
                callback(Enumerable.Empty<IChannel>());
                return;
            }
            Users.GetUser(userId, u => GetChannelsUserCallback(u, callback));
        }

        public void GetChannels(IUser user, Action<IEnumerable<IChannel>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            Users.GetUser(user, u => GetChannelsUserCallback(u, callback));
        }

        private void GetChannelsUserCallback(IUser u, Action<IEnumerable<IChannel>> callback) {
            if(u is not User user) {
                callback(Enumerable.Empty<IChannel>());
                return;
            }

            user.GetChannels(c => Channels.GetChannels(c, callback));
        }

        public void JoinChannel(IChannel channel, ISession session) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            HasSession(channel, session, hasSession => {
                if(hasSession)
                    return;

                // SessionJoin and UserJoin should be combined
                HasUser(channel, session.User, HasUser => {
                    Dispatcher.DispatchEvent(
                        this,
                        HasUser
                            ? new ChannelSessionJoinEvent(channel, session)
                            : new ChannelUserJoinEvent(channel, session)
                    );
                });
            });
        }

        public void LeaveChannel(IChannel channel, IUser user, UserDisconnectReason reason = UserDisconnectReason.Unknown) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            HasUser(channel, user, hasUser => {
                if(hasUser)
                    Dispatcher.DispatchEvent(this, new ChannelUserLeaveEvent(channel, user, reason));
            });
        }

        public void LeaveChannel(IChannel channel, ISession session) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            HasSession(channel, session, hasSession => {
                // UserLeave and SessionLeave should be combined
                CountUserSessions(channel, session.User, sessionCount => {
                    Dispatcher.DispatchEvent(
                        this,
                        sessionCount <= 1
                            ? new ChannelUserLeaveEvent(channel, session.User, UserDisconnectReason.Leave)
                            : new ChannelSessionLeaveEvent(channel, session)
                    );
                });
            });
        }

        public void LeaveChannels(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            Channels.GetChannels(channels => {
                foreach(IChannel channel in channels)
                    LeaveChannel(channel, session);
            });
        }

        public void HandleEvent(object sender, IEvent evt) {
            IEnumerable<IUser> targets = null;

            switch(evt) {
                case UserUpdateEvent uue: // fetch up to date user info
                    // THIS IS VERY ILLEGAL
                    GetChannelsByUserId(evt.UserId, channels => GetUsers(channels, users => targets = users));
                    Users.GetUser(uue.UserId, user => {
                        if(user != null)
                            evt = new UserUpdateEvent(user, uue);
                    });
                    break;

                case ChannelUserJoinEvent cje:
                    // THIS DOES NOT DO WHAT YOU WANT IT TO DO
                    // I THINK
                    // it really doesn't, figure out how to leave channels when MCHAN isn't active for the session
                    //if((Sessions.GetCapabilities(cje.User) & ClientCapability.MCHAN) == 0)
                    //    LeaveChannel(cje.Channel, cje.User, UserDisconnectReason.Leave);
                    break;

                case ChannelUserLeaveEvent cle: // Should ownership just be passed on to another user instead of Destruction?
                    Channels.GetChannel(evt.ChannelName, channel => {
                        if(channel.IsTemporary && evt.UserId == channel.OwnerId)
                            Channels.Remove(channel);
                    });
                    break;

                case SessionDestroyEvent sde:
                    Users.GetUser(sde.UserId, user => {
                        if(user == null)
                            return;
                        Sessions.GetSessionCount(user, sessionCount => {
                            if(sessionCount < 1)
                                Users.Disconnect(user, UserDisconnectReason.TimeOut);
                        });
                    });
                    break;
            }

            // Any forwarding that happens here should probably Go

            if(targets == null && evt.ChannelName != null)
                GetUsers(evt.ChannelName, users => targets = users);

            if(targets != null)
                Sessions.GetSessions(targets, sessions => {
                    foreach(ISession session in sessions)
                        session.HandleEvent(sender, evt);
                });
        }
    }
}
