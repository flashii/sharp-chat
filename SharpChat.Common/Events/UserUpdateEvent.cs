using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class UserUpdateEvent : Event {
        public const string TYPE = @"user:update";

        public string OldUserName { get; }
        public string NewUserName { get; }

        public Colour OldColour { get; }
        public Colour? NewColour { get; }

        public int? OldRank { get; }
        public int? NewRank { get; }

        public string OldNickName { get; }
        public string NewNickName { get; }

        public UserPermissions OldPerms { get; }
        public UserPermissions? NewPerms { get; }

        public UserStatus OldStatus { get; }
        public UserStatus? NewStatus { get; }

        public string OldStatusMessage { get; }
        public string NewStatusMessage { get; }

        public bool HasUserName => NewUserName != null;
        public bool HasNickName => NewNickName != null;
        public bool HasStatusMessage => NewStatusMessage != null;

        public UserUpdateEvent(
            IUser user,
            string userName = null,
            Colour? colour = null,
            int? rank = null,
            string nickName = null,
            UserPermissions? perms = null,
            UserStatus? status = null,
            string statusMessage = null
        ) : base(user ?? throw new ArgumentNullException(nameof(user))) {
            OldUserName = user.UserName;
            if(!OldUserName.Equals(userName))
                NewUserName = userName;

            OldColour = user.Colour;
            if(!OldColour.Equals(colour))
                NewColour = colour;

            OldRank = user.Rank;
            if(OldRank != rank)
                NewRank = rank;

            OldNickName = user.NickName;
            if(!OldNickName.Equals(nickName))
                NewNickName = nickName;

            OldPerms = user.Permissions;
            if(OldPerms != perms)
                NewPerms = perms;

            OldStatus = user.Status;
            if(OldStatus != status)
                NewStatus = status;

            OldStatusMessage = user.StatusMessage;
            if(!OldStatusMessage.Equals(statusMessage))
                NewStatusMessage = statusMessage;
        }

        public UserUpdateEvent(IUser user, UserUpdateEvent uue)
            : this(
                  user,
                  uue.NewUserName,
                  uue.NewColour,
                  uue.NewRank,
                  uue.NewNickName,
                  uue.NewPerms,
                  uue.NewStatus,
                  uue.NewStatusMessage
              ) { }
    }
}
