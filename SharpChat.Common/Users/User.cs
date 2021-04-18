using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Users {
    public class User : IUser, IEventHandler {
        public long UserId { get; }
        public string UserName { get; private set; }
        public Colour Colour { get; private set; }
        public int Rank { get; private set; }
        public string NickName { get; private set; }
        public UserPermissions Permissions { get; private set; }
        public UserStatus Status { get; private set; } = UserStatus.Online;
        public string StatusMessage { get; private set; }

        private readonly object Sync = new object();
        private HashSet<string> Channels { get; } = new HashSet<string>();

        public User(
            long userId,
            string userName,
            Colour colour,
            int rank,
            UserPermissions perms,
            UserStatus status,
            string statusMessage,
            string nickName
        ) {
            UserId = userId;
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Colour = colour;
            Rank = rank;
            Permissions = perms;
            Status = status;
            StatusMessage = statusMessage ?? string.Empty;
            NickName = nickName ?? string.Empty;
        }

        public User(IUserAuthResponse auth) {
            UserId = auth.UserId;
            ApplyAuth(auth);
        }

        public void ApplyAuth(IUserAuthResponse auth) {
            UserName = auth.UserName;

            if(Status == UserStatus.Offline)
                Status = UserStatus.Online;

            Colour = auth.Colour;
            Rank = auth.Rank;
            Permissions = auth.Permissions;
        }

        public override string ToString()
            => $@"<User {UserId}#{UserName}>";

        public bool Equals(IUser other)
            => other != null && other.UserId == UserId;

        public bool HasChannel(IChannel channel) {
            if(channel == null)
                return false;
            lock(Sync)
                return Channels.Contains(channel.ChannelId);
        }

        public void GetChannels(Action<IEnumerable<string>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));
            lock(Sync)
                callback.Invoke(Channels);
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync) {
                switch(evt) {
                    case ChannelUserJoinEvent cje:
                        Channels.Add(evt.ChannelId);
                        break;
                    case ChannelUserLeaveEvent cle:
                        Channels.Remove(evt.ChannelId);
                        break;

                    case UserUpdateEvent uue:
                        if(uue.HasUserName)
                            UserName = uue.NewUserName;
                        if(uue.NewColour.HasValue)
                            Colour = uue.NewColour.Value;
                        if(uue.NewRank.HasValue)
                            Rank = uue.NewRank.Value;
                        if(uue.HasNickName)
                            NickName = uue.NewNickName;
                        if(uue.NewPerms.HasValue)
                            Permissions = uue.NewPerms.Value;
                        if(uue.NewStatus.HasValue)
                            Status = uue.NewStatus.Value;
                        if(uue.HasStatusMessage)
                            StatusMessage = uue.NewStatusMessage;
                        break;
                    case UserDisconnectEvent _:
                        Status = UserStatus.Offline;
                        break;
                }
            }
        }
    }
}
