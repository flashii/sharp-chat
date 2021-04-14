using Hamakaze;
using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.Users.Bump;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace SharpChat.DataProvider.Misuzu.Users.Bump {
    public class MisuzuUserBumpClient : IUserBumpClient {
        private MisuzuDataProvider DataProvider { get; }
        private HttpClient HttpClient { get; }

        private const string URL = @"/bump";

        public MisuzuUserBumpClient(MisuzuDataProvider dataProvider, HttpClient httpClient) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void SubmitBumpUsers(
            SessionManager sessions,
            IEnumerable<IUser> users,
            Action onSuccess = null,
            Action<Exception> onFailure = null
        ) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(!users.Any())
                return;

            List<MisuzuUserBumpInfo> infos = new List<MisuzuUserBumpInfo>();

            // this is illegal
            foreach(IUser user in users) {
                sessions.GetRemoteAddresses(user, addrs => {
                    IPAddress addr = addrs.FirstOrDefault();
                    if(addr == default)
                        return;
                    infos.Add(new MisuzuUserBumpInfo(user, addr));
                });
            }

            byte[] data = JsonSerializer.SerializeToUtf8Bytes(infos);

            HttpRequestMessage request = new HttpRequestMessage(HttpRequestMessage.POST, DataProvider.GetURL(URL));
            request.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(data));
            request.SetBody(data);

            HttpClient.SendRequest(
                request,
                disposeRequest: false,
                onComplete: (t, r) => { request.Dispose(); onSuccess?.Invoke(); },
                onError: (t, e) => {
                    Logger.Write(@"User bump request failed. Retrying once...");
                    Logger.Debug(e);

                    HttpClient.SendRequest(
                        request,
                        onComplete: (t, r) => {
                            Logger.Write(@"Second user bump attempt succeeded!");
                            onSuccess?.Invoke();
                        },
                        onError: (t, e) => {
                            Logger.Write(@"User bump request failed again.");
                            Logger.Debug(e);
                            onFailure?.Invoke(e);
                        }
                    );
                }
            );
        }
    }
}
