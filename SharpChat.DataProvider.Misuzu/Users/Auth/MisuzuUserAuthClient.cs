using Hamakaze;
using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.IO;
using System.Text.Json;

namespace SharpChat.DataProvider.Misuzu.Users.Auth {
    public class MisuzuUserAuthClient : IUserAuthClient {
        private MisuzuDataProvider DataProvider { get; }
        private HttpClient HttpClient { get; }

        private const string URL = @"/verify";

        public MisuzuUserAuthClient(MisuzuDataProvider dataProvider, HttpClient httpClient) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void AttemptAuth(UserAuthRequest request, Action<IUserAuthResponse> onSuccess, Action<Exception> onFailure) {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

#if DEBUG
            if(request.UserId >= 10000) {
                onSuccess.Invoke(new MisuzuUserAuthResponse {
                    Success = true,
                    UserId = request.UserId,
                    UserName = @"Misaka-" + (request.UserId - 10000),
                    ColourRaw = (RNG.Next(0, 255) << 16) | (RNG.Next(0, 255) << 8) | RNG.Next(0, 255),
                    Rank = 0,
                    SilencedUntil = DateTimeOffset.MinValue,
                    Permissions = UserPermissions.SendMessage | UserPermissions.EditOwnMessage | UserPermissions.DeleteOwnMessage,
                });
                return;
            }
#endif
            MisuzuUserAuthRequest mar = new(request);

            HttpRequestMessage req = new(HttpRequestMessage.POST, DataProvider.GetURL(URL));
            req.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(mar));
            req.SetBody(JsonSerializer.SerializeToUtf8Bytes(mar));

            HttpClient.SendRequest(
                req,
                onComplete: (t, r) => {
                    using MemoryStream ms = new();
                    r.Body.CopyTo(ms);
                    MisuzuUserAuthResponse res = JsonSerializer.Deserialize<MisuzuUserAuthResponse>(ms.ToArray());
                    if(res.Success)
                        onSuccess.Invoke(res);
                    else
                        onFailure.Invoke(new UserAuthFailedException(res.Reason));
                },
                onError: (t, e) => {
                    Logger.Write(@"An error occurred during authentication.");
                    Logger.Debug(e);
                    onFailure.Invoke(e);
                }
            );
        }
    }
}
