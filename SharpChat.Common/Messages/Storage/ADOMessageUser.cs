using SharpChat.Database;
using SharpChat.Users;
using System;

namespace SharpChat.Messages.Storage {
    public class ADOMessageUser : IUser {
        public long UserId { get; }
        public string UserName { get; }
        public Colour Colour { get; }
        public int Rank { get; }
        public string NickName { get; }
        public UserPermissions Permissions { get; }
        public UserStatus Status => UserStatus.Unknown;
        public string StatusMessage => string.Empty;

        public ADOMessageUser(IDatabaseReader reader) {
            if(reader == null)
                throw new ArgumentNullException(nameof(reader));
            UserId = reader.ReadI64(@"msg_sender_id");
            UserName = reader.ReadString(@"msg_sender_name");
            Colour = new(reader.ReadI32(@"msg_sender_colour"));
            Rank = reader.ReadI32(@"msg_sender_rank");
            NickName = reader.IsNull(@"msg_sender_nick") ? null : reader.ReadString(@"msg_sender_nick");
            Permissions = (UserPermissions)reader.ReadI32(@"msg_sender_perms");
        }

        public bool Equals(IUser other)
            => other != null && other.UserId == UserId;

        public override string ToString()
            => $@"<ADOMessageUser {UserId}#{UserName}>";
    }
}
