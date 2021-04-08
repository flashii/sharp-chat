using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Channels {
    public class ChannelUserRelations : IEventHandler {
        private IEventDispatcher Dispatcher { get; }
        private ChannelManager Channels { get; }
        private UserManager Users { get; }
        private SessionManager Sessions { get; }
        private MessageManager Messages { get; }
        private readonly object Sync = new object();

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

        public bool HasUser(IChannel channel, IUser user) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            if(Channels.GetChannel(channel) is not Channel c)
                return false;

            return c.HasUser(user);
        }

        public bool HasSession(IChannel channel, ISession session) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            if(Channels.GetChannel(channel) is not Channel c)
                return false;

            return c.HasSession(session);
        }

        public int CountUsers(IChannel channel) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));

            if(Channels.GetChannel(channel) is not Channel c)
                return 0;

            return c.CountUsers();
        }

        public int CountUserSessions(IChannel channel, IUser user) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            if(Channels.GetChannel(channel) is not Channel c)
                return 0;

            return c.CountUserSessions(user);
        }

        public void GetUsers(IChannel channel, Action<IEnumerable<IUser>> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            if(Channels.GetChannel(channel) is Channel c)
                c.GetUserIds(ids => Users.GetUsers(ids, callback));
        }

        public void GetUsers(IEnumerable<IChannel> channels, Action<IEnumerable<IUser>> callback) {
            if(channels == null)
                throw new ArgumentNullException(nameof(channels));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            HashSet<long> ids = new HashSet<long>();

            foreach(IChannel channel in channels)
                if(Channels.GetChannel(channel) is Channel c)
                    c.GetUserIds(u => {
                        foreach(long id in u)
                            ids.Add(id);
                    });

            Users.GetUsers(ids, callback);
        }

        public void GetUsers(IUser user, Action<IEnumerable<IUser>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync) {
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
        }

        public void GetChannels(IUser user, Action<IEnumerable<IChannel>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            if(Users.GetUser(user) is User u)
                u.GetChannels(cn => Channels.GetChannels(cn, callback));
        }

        public void GetChannels(ISession session, Action<IEnumerable<IChannel>> callback) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            //
        }

        public void JoinChannel(IChannel channel, ISession session) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            if(!HasSession(channel, session))
                Dispatcher.DispatchEvent(
                    this,
                    HasUser(channel, session.User)
                        ? new ChannelSessionJoinEvent(channel, session)
                        : new ChannelUserJoinEvent(channel, session)
                );
        }

        public void LeaveChannel(IChannel channel, IUser user, UserDisconnectReason reason = UserDisconnectReason.Unknown) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            if(HasUser(channel, user))
                Dispatcher.DispatchEvent(this, new ChannelUserLeaveEvent(channel, user, reason));
        }

        public void LeaveChannel(IChannel channel, ISession session) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            if(HasSession(channel, session))
                Dispatcher.DispatchEvent(
                    this,
                    CountUserSessions(channel, session.User) <= 1
                        ? new ChannelUserLeaveEvent(channel, session.User, UserDisconnectReason.Leave)
                        : new ChannelSessionLeaveEvent(channel, session)
                );
        }

        public void LeaveChannels(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            lock(Sync)
                Channels.GetChannels(channels => {
                    foreach(IChannel channel in channels)
                        if(HasSession(channel, session))
                            LeaveChannel(channel, session);
                });
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync) {
                IEnumerable<IUser> targets = null;

                switch(evt) {
                    case UserUpdateEvent uue: // fetch up to date user info
                        GetChannels(evt.User, channels => GetUsers(channels, users => targets = users));

                        IUser uueUser = Users.GetUser(uue.User);
                        if(uueUser != null)
                            evt = new UserUpdateEvent(uueUser, uue);
                        break;

                    case ChannelUserJoinEvent cje:
                        // THIS DOES NOT DO WHAT YOU WANT IT TO DO
                        // I THINK
                        // it really doesn't, figure out how to leave channels when MCHAN isn't active for the session
                        //if((Sessions.GetCapabilities(cje.User) & ClientCapability.MCHAN) == 0)
                        //    LeaveChannel(cje.Channel, cje.User, UserDisconnectReason.Leave);
                        break;

                    case ChannelUserLeaveEvent cle: // Should ownership just be passed on to another user instead of Destruction?
                        IChannel channel = Channels.GetChannel(evt.Channel);
                        if(channel.IsTemporary && evt.User.Equals(channel.Owner))
                            Channels.Remove(channel);
                        break;

                    case SessionDestroyEvent sde:
                        if(Sessions.GetSessionCount(sde.User) < 1)
                            Users.Disconnect(sde.User, UserDisconnectReason.TimeOut);
                        break;
                }

                if(targets == null && evt.Channel != null)
                    GetUsers(evt.Channel, users => targets = users);

                if(targets != null)
                    Sessions.GetSessions(targets, sessions => {
                        foreach(ISession session in sessions)
                            session.HandleEvent(sender, evt);
                    });
            }
        }
    }
}
