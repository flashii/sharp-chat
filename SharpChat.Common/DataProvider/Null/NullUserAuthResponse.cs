using SharpChat.Users;
using SharpChat.Users.Auth;
using System;

namespace SharpChat.DataProvider.Null {
    public class NullUserAuthResponse : IUserAuthResponse {
        public long UserId { get; }
        public string UserName { get; }
        public int Rank { get; }
        public Colour Colour { get; }
        public UserPermissions Permissions { get; }
        public DateTimeOffset SilencedUntil => DateTimeOffset.MinValue;

        public NullUserAuthResponse(UserAuthRequest uar) {
            UserId = uar.UserId;
            UserName = $@"Misaka-{uar.UserId}";
            Rank = (int)(uar.UserId % 10);
            Random rng = new((int)uar.UserId);
            Colour = new(rng.Next());
            Permissions = (UserPermissions)rng.Next();
        }
    }
}
