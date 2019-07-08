﻿using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat
{
    public class FlashiiAuth
    {
        [JsonProperty(@"success")]
        public bool Success { get; set; }

        [JsonProperty(@"user_id")]
        public int UserId { get; set; }

        [JsonProperty(@"username")]
        public string Username { get; set; }

        [JsonProperty(@"colour")]
        public string Colour { get; set; }

        [JsonProperty(@"default_channel")]
        public string DefaultChannel { get; set; }

        [JsonProperty(@"hierarchy")]
        public int Hierarchy { get; set; }

        [JsonProperty(@"is_mod")]
        public bool IsModerator { get; set; }

        [JsonProperty(@"can_change_nick")]
        public bool CanChangeNick { get; set; }

        [JsonProperty(@"can_create_chan")]
        public SockChatUserChannel CanCreateChannels { get; set; }

        public static FlashiiAuth Attempt(int userId, string token, string ip, string endpoint = "https://flashii.net/_sockchat.php?user_id={0}&token={1}&ip={2}&hash={3}")
        {
            try
            {
                using (HMACSHA256 hash = new HMACSHA256(File.Exists(@"login_key.txt") ? File.ReadAllBytes(@"login_key.txt") : Encoding.ASCII.GetBytes(@"woomy")))
                using (WebClient wc = new WebClient())
                    return JsonConvert.DeserializeObject<FlashiiAuth>(wc.DownloadString(string.Format(
                        endpoint, userId, token, ip,
                        hash.ComputeHash(Encoding.ASCII.GetBytes($@"{userId}#{token}#{ip}")).ToHexString()
                    )));
            }
            catch
            {
                return new FlashiiAuth { Success = false };
            }
        }
    }
}
