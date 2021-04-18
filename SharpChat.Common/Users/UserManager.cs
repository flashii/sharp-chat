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
                if(Users.Any(u => u.UserId == uce.UserId))
                    throw new ArgumentException(@"User already registered?????", nameof(uce));

                Users.Add(new User(
                    uce.UserId,
                    uce.Name,
                    uce.Colour,
                    uce.Rank,
                    uce.Permissions,
                    uce.Status,
                    uce.StatusMessage,
                    uce.NickName
                ));
            }
        }

        public void Disconnect(IUser user, UserDisconnectReason reason = UserDisconnectReason.Unknown) {
            if(user == null)
                return;
            Dispatcher.DispatchEvent(this, new UserDisconnectEvent(user, reason));
        }

        private void OnDisconnect(object sender, UserDisconnectEvent ude) {
            GetUser(ude.UserId, user => {
                if(user == null)
                    return;
                if(user is IEventHandler ueh)
                    ueh.HandleEvent(sender, ude);
            });
        }

        private void OnUpdate(object sender, UserUpdateEvent uue) {
            GetUser(uue.UserId, user => {
                if(user == null)
                    return;
                if(user is IEventHandler ueh)
                    ueh.HandleEvent(sender, uue);
            });
        }

        public void GetUser(Func<IUser, bool> predicate, Action<IUser> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Users.FirstOrDefault(predicate));
        }

        public void GetUser(long userId, Action<IUser> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetUser(u => u.UserId == userId, callback);
        }

        public void GetUser(IUser user, Action<IUser> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetUser(user.Equals, callback);
        }

        // feel like this one should be obsoleted entirely 
        public void GetUser(string username, Action<IUser> callback, bool includeNickName = true, bool includeDisplayName = true) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            if(string.IsNullOrWhiteSpace(username))
                return;
            GetUser(u => u.UserName.ToLowerInvariant() == username
                    || (includeNickName && u.NickName?.ToLowerInvariant() == username)
                    /*|| (includeDisplayName && u.GetDisplayName().ToLowerInvariant() == username)*/, callback);
        }

        public void GetUsers(Action<IEnumerable<IUser>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Users);
        }

        public void GetUsers(Func<IUser, bool> predicate, Action<IEnumerable<IUser>> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback(Users.Where(predicate));
        }

        public void GetUsers(int minRank, Action<IEnumerable<IUser>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetUsers(u => u.Rank >= minRank, callback);
        }

        public void GetUsers(IEnumerable<long> ids, Action<IEnumerable<IUser>> callback) {
            if(ids == null)
                throw new ArgumentNullException(nameof(ids));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            GetUsers(u => ids.Contains(u.UserId), callback);
        }

        public void Connect(IUserAuthResponse uar, Action<IUser> callback) {
            if(uar == null)
                throw new ArgumentNullException(nameof(uar));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync) {
                GetUser(uar.UserId, user => {
                    if(user == null)
                        Create(uar.UserId, uar.UserName, uar.Colour, uar.Rank, uar.Permissions, callback: callback);
                    else {
                        Update(user, uar.UserName, uar.Colour, uar.Rank, uar.Permissions);
                        callback(user);
                    }
                });
            }
        }

        public void Create(
            long userId,
            string userName,
            Colour colour,
            int rank,
            UserPermissions perms,
            UserStatus status = UserStatus.Online,
            string statusMessage = null,
            string nickName = null,
            Action<IUser> callback = null
        ) {
            if(userName == null)
                throw new ArgumentNullException(nameof(userName));

            lock(Sync) {
                User user = new User(userId, userName, colour, rank, perms, status, statusMessage, nickName);
                Users.Add(user);
                Dispatcher.DispatchEvent(this, new UserConnectEvent(user));
                callback?.Invoke(user);
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
            switch(evt) {
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
