using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.Text.Json.Serialization;

namespace SharpChat.DataProvider.Misuzu.Users.Auth {
    public class MisuzuUserAuthResponse : IUserAuthResponse {
        [JsonPropertyName(@"success")]
        public bool Success { get; set; }

        [JsonPropertyName(@"reason")]
        public string Reason { get; set; } = @"none";

        [JsonPropertyName(@"user_id")]
        public long UserId { get; set; }

        [JsonPropertyName(@"username")]
        public string UserName { get; set; }

        [JsonPropertyName(@"colour_raw")]
        public int ColourRaw { get; set; }

        [JsonIgnore]
        public Colour Colour => new(ColourRaw);

        [JsonPropertyName(@"hierarchy")]
        public int Rank { get; set; }

        [JsonPropertyName(@"is_silenced")]
        public DateTimeOffset SilencedUntil { get; set; }

        [JsonPropertyName(@"perms")]
        public UserPermissions Permissions { get; set; }
    }
}
