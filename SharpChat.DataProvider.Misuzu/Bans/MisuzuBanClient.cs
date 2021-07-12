using Hamakaze;
using SharpChat.Bans;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Web;

namespace SharpChat.DataProvider.Misuzu.Bans {
    public class MisuzuBanClient : IBanClient {
        private const string STRING = @"givemethebeans";

        private MisuzuDataProvider DataProvider { get; }
        private HttpClient HttpClient { get; }

        private const string URL = @"/bans";
        private const string URL_CHECK = URL + @"/check";
        private const string URL_CREATE = URL + @"/create";
        private const string URL_REMOVE = URL + @"/remove";

        public MisuzuBanClient(MisuzuDataProvider dataProvider, HttpClient httpClient) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void GetBanList(Action<IEnumerable<IBanRecord>> onSuccess, Action<Exception> onFailure = null) {
            HttpRequestMessage req = new(HttpRequestMessage.GET, DataProvider.GetURL(URL));
            req.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(STRING));

            HttpClient.SendRequest(
                req,
                onComplete: (t, r) => onSuccess.Invoke(JsonSerializer.Deserialize<IEnumerable<MisuzuBanRecord>>(r.GetBodyBytes())),
                (t, e) => { Logger.Debug(e); onFailure?.Invoke(e); }
            );
        }

        public void CheckBan(long userId, IPAddress ipAddress, Action<IBanRecord> onSuccess, Action<Exception> onFailure = null) {
            HttpRequestMessage req = new(
                HttpRequestMessage.GET,
                string.Format(@"{0}?a={1}&u={2}", DataProvider.GetURL(URL_CHECK), ipAddress, userId)
            );
            req.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(string.Format(@"check#{0}#{1}", ipAddress, userId)));

            HttpClient.SendRequest(
                req,
                (t, r) => onSuccess.Invoke(JsonSerializer.Deserialize<MisuzuBanRecord>(r.GetBodyBytes())),
                (t, e) => { Logger.Debug(e); onFailure?.Invoke(e); }
            );
        }

        public void CreateBan(long userId, long modId, bool perma, TimeSpan duration, string reason, Action<bool> onSuccess = null, Action<Exception> onFailure = null) {
            reason ??= string.Empty;
            if(modId < 1)
                modId = DataProvider.ActorId;

            HttpRequestMessage req = new(HttpRequestMessage.POST, DataProvider.GetURL(URL_CREATE));
            req.SetHeader(@"Content-Type", @"application/x-www-form-urlencoded");
            req.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(string.Format(
                @"create#{0}#{1}#{2}#{3}#{4}",
                userId, modId, duration.TotalSeconds, perma ? '1' : '0', reason
            )));
            req.SetBody(string.Format(
                @"u={0}&m={1}&d={2}&p={3}&r={4}",
                userId, modId, duration.TotalSeconds, perma ? '1' : '0', HttpUtility.UrlEncode(reason)
            ));

            HttpClient.SendRequest(
                req,
                (t, r) => onSuccess?.Invoke(r.StatusCode == 201),
                (t, e) => { Logger.Debug(e); onFailure?.Invoke(e); }
            );
        }

        public void RemoveBan(string userName, Action<bool> onSuccess, Action<Exception> onFailure = null) {
            RemoveBan(@"user", userName, onSuccess, onFailure);
        }

        public void RemoveBan(IPAddress ipAddress, Action<bool> onSuccess, Action<Exception> onFailure = null) {
            RemoveBan(@"ip", ipAddress.ToString(), onSuccess, onFailure);
        }

        private void RemoveBan(string type, string subject, Action<bool> onSuccess, Action<Exception> onFailure = null) {
            HttpRequestMessage req = new(
                HttpRequestMessage.DELETE,
                string.Format(@"{0}?t={1}&s={2}", DataProvider.GetURL(URL_REMOVE), type, subject)
            );
            req.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(string.Format(@"remove#{0}#{1}", type, subject)));

            HttpClient.SendRequest(
                req,
                (t, r) => onSuccess.Invoke(r.StatusCode == 204),
                (t, e) => { Logger.Debug(e); onFailure?.Invoke(e); }
            );
        }
    }
}
