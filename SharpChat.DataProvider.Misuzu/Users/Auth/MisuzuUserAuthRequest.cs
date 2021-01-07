﻿using SharpChat.Users.Auth;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.DataProvider.Misuzu.Users.Auth {
    internal class MisuzuUserAuthRequest {
        [JsonPropertyName(@"user_id")]
        public long UserId => AuthRequest.UserId;

        [JsonPropertyName(@"token")]
        public string Token => AuthRequest.Token;

        [JsonPropertyName(@"ip")]
        public string IPAddress => AuthRequest.RemoteAddress.ToString();

        [JsonIgnore]
        public string Hash
            => string.Join(@"#", UserId, Token, IPAddress).GetSignedHash();

        public byte[] GetJSON()
            => JsonSerializer.SerializeToUtf8Bytes(this);

        private UserAuthRequest AuthRequest { get; }

        public MisuzuUserAuthRequest(UserAuthRequest uar) {
            AuthRequest = uar ?? throw new ArgumentNullException(nameof(uar));
        }
    }
}
