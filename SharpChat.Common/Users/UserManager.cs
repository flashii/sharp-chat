using SharpChat.Events;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Users {
    public class UserManager : IEventHandler {
        private List<User> Users { get; } = new List<User>();
        private IEventDispatcher Dispatcher { get; }
        private readonly object Sync = new object();

        public UserManager(IEventDispatcher dispatcher) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        private void OnConnect(object sender, UserConnectEvent uce) {
            if(sender == this)
                return;

            lock(Sync) {
                if(Contains(uce.User))
                    throw new ArgumentException(@"User already registered?????", nameof(uce));

                Users.Add(new User(
                    uce.User.UserId,
                    uce.User.UserName,
                    uce.User.Colour,
                    uce.User.Rank,
                    uce.User.Permissions,
                    uce.Status,
                    uce.StatusMessage,
                    uce.User.NickName
                ));
            }
        }

        public void Disconnect(IUser user, UserDisconnectReason reason = UserDisconnectReason.Unknown) {
            if(user == null)
                return;
            lock(Sync)
                Dispatcher.DispatchEvent(this, new UserDisconnectEvent(user, reason));
        }

        private void OnDisconnect(object sender, UserDisconnectEvent ude) {
            lock(Sync) {
                IUser user = GetUser(ude.User.UserId);
                if(user == null)
                    return;
                if(user is IEventHandler ueh)
                    ueh.HandleEvent(sender, ude);
            }
        }

        private void OnUpdate(object sender, UserUpdateEvent uue) {
            lock(Sync) {
                IUser user = GetUser(uue.User.UserId);
                if(user is IEventHandler ueh)
                    ueh.HandleEvent(sender, uue);
            }
        }

        public bool Contains(IUser user) {
            if(user == null)
                return false;

            lock(Sync)
                return Users.Contains(user) // the below should probably use .Equals
                    || Users.Any(x => x.Equals(user) || x.UserName.ToLowerInvariant() == user.UserName.ToLowerInvariant());
        }

        public IUser GetUser(long userId) {
            lock(Sync)
                return Users.FirstOrDefault(x => x.UserId == userId);
        }

        public IUser GetUser(string username, bool includeNickName = true, bool includeDisplayName = true) {
            if(string.IsNullOrWhiteSpace(username))
                return null;
            username = username.ToLowerInvariant();

            lock(Sync)
                return Users.FirstOrDefault(x => x.UserName.ToLowerInvariant() == username
                    || (includeNickName && x.NickName?.ToLowerInvariant() == username)
                    /*|| (includeDisplayName && x.GetDisplayName().ToLowerInvariant() == username)*/);
        }

        public IUser GetUser(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            lock(Sync) {
                if(user is User u && Users.Contains(u))
                    return u;
                return Users.FirstOrDefault(u => u.Equals(user));
            }
        }

        public void GetUsers(int minRank, Action<IEnumerable<IUser>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback.Invoke(Users.Where(u => u.Rank >= minRank));
        }

        public void GetUsers(IEnumerable<long> ids, Action<IEnumerable<IUser>> callback) {
            if(ids == null)
                throw new ArgumentNullException(nameof(ids));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback.Invoke(Users.Where(u => ids.Contains(u.UserId)));
        }

        public void GetUsers(Action<IEnumerable<IUser>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback.Invoke(Users);
        }

        public IUser Connect(IUserAuthResponse uar) {
            lock(Sync) {
                IUser user = GetUser(uar.UserId);
                if(user == null)
                    return Create(uar.UserId, uar.UserName, uar.Colour, uar.Rank, uar.Permissions);

                Update(user, uar.UserName, uar.Colour, uar.Rank, uar.Permissions);
                return user;
            }
        }

        public IUser Create(
            long userId,
            string userName,
            Colour colour,
            int rank,
            UserPermissions perms,
            UserStatus status = UserStatus.Online,
            string statusMessage = null,
            string nickName = null
        ) {
            if(userName == null)
                throw new ArgumentNullException(nameof(userName));

            lock(Sync) {
                User user = new User(userId, userName, colour, rank, perms, status, statusMessage, nickName);
                Users.Add(user);
                Dispatcher.DispatchEvent(this, new UserConnectEvent(user));
                return user;
            }
        }

        public void Update(
            IUser user,
            string userName = null,
            Colour? colour = null,
            int? rank = null,
            UserPermissions? perms = null,
            UserStatus? status = null,
            string statusMessage = null,
            string nickName = null
        ) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            lock(Sync) {
                if(userName != null && user.UserName == userName)
                    userName = null;

                if(colour.HasValue && user.Colour.Equals(colour))
                    colour = null;

                if(rank.HasValue && user.Rank == rank.Value)
                    rank = null;

                if(nickName != null) {
                    string prevNickName = user.NickName ?? string.Empty;

                    if(nickName == prevNickName) {
                        nickName = null;
                    } else {
                        string nextUserName = userName ?? user.UserName;
                        if(nickName == nextUserName) {
                            nickName = null;
                        } else {
                            // cleanup
                        }
                    }
                }

                if(perms.HasValue && user.Permissions == perms.Value)
                    perms = null;

                if(status.HasValue && user.Status == status.Value)
                    status = null;

                if(statusMessage != null && user.StatusMessage == statusMessage) {
                    statusMessage = null;
                } else {
                    // cleanup
                }

                Dispatcher.DispatchEvent(this, new UserUpdateEvent(user, userName, colour, rank, nickName, perms, status, statusMessage));
            }
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync)
                switch(evt) {
                    // Send to all users with minimum rank
                    case ChannelCreateEvent cce:
                        break;
                    case ChannelUpdateEvent cue:
                        break;
                    case ChannelDeleteEvent cde:
                        break;

                    case UserConnectEvent uce:
                        OnConnect(sender, uce);
                        break;
                    case UserUpdateEvent uue:
                        OnUpdate(sender, uue);
                        break;
                    case UserDisconnectEvent ude:
                        OnDisconnect(sender, ude);
                        break;
                }
        }
    }
}
