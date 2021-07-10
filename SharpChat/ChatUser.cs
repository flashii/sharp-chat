using SharpChat.Flashii;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpChat {
    public class BasicUser : IEquatable<BasicUser> {
        private const int RANK_NO_FLOOD = 9;

        public long UserId { get; set; }
        public string Username { get; set; }
        public ChatColour Colour { get; set; }
        public int Rank { get; set; }
        public string Nickname { get; set; }
        public ChatUserPermissions Permissions { get; set; }
        public ChatUserStatus Status { get; set; } = ChatUserStatus.Online;
        public string StatusMessage { get; set; }

        public bool HasFloodProtection
            => Rank < RANK_NO_FLOOD;

        public bool Equals([AllowNull] BasicUser other)
            => UserId == other.UserId;

        public string DisplayName {
            get {
                StringBuilder sb = new StringBuilder();

                if(Status == ChatUserStatus.Away)
                    sb.AppendFormat(@"&lt;{0}&gt;_", StatusMessage.Substring(0, Math.Min(StatusMessage.Length, 5)).ToUpperInvariant());

                if(string.IsNullOrWhiteSpace(Nickname))
                    sb.Append(Username);
                else {
                    sb.Append('~');
                    sb.Append(Nickname);
                }

                return sb.ToString();
            }
        }

        public bool Can(ChatUserPermissions perm, bool strict = false) {
            ChatUserPermissions perms = Permissions & perm;
            return strict ? perms == perm : perms > 0;
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append('\t');
            sb.Append(DisplayName);
            sb.Append('\t');
            sb.Append(Colour);
            sb.Append('\t');
            sb.Append(Rank);
            sb.Append(' ');
            sb.Append(Can(ChatUserPermissions.KickUser) ? '1' : '0');
            sb.Append(@" 0 ");
            sb.Append(Can(ChatUserPermissions.SetOwnNickname) ? '1' : '0');
            sb.Append(' ');
            sb.Append(Can(ChatUserPermissions.CreateChannel | ChatUserPermissions.SetChannelPermanent, true) ? 2 : (
                Can(ChatUserPermissions.CreateChannel) ? 1 : 0
            ));

            return sb.ToString();
        }
    }

    public class ChatUser : BasicUser, IPacketTarget {
        public DateTimeOffset SilencedUntil { get; set; }

        private readonly List<ChatUserSession> Sessions = new List<ChatUserSession>();
        private readonly List<ChatChannel> Channels = new List<ChatChannel>();

        public readonly ChatRateLimiter RateLimiter = new ChatRateLimiter();

        public string TargetName => @"@log";

        [Obsolete]
        public ChatChannel Channel {
            get {
                lock(Channels)
                    return Channels.FirstOrDefault();
            }
        }

        // This needs to be a session thing
        public ChatChannel CurrentChannel { get; private set; }

        public bool IsSilenced
            => SilencedUntil != null && DateTimeOffset.UtcNow - SilencedUntil <= TimeSpan.Zero;

        public bool HasSessions {
            get {
                lock(Sessions)
                    return Sessions.Where(c => !c.HasTimedOut && !c.IsDisposed).Any();
            }
        }

        public int SessionCount {
            get {
                lock (Sessions)
                    return Sessions.Where(c => !c.HasTimedOut && !c.IsDisposed).Count();
            }
        }

        public IEnumerable<IPAddress> RemoteAddresses {
            get {
                lock(Sessions)
                    return Sessions.Select(c => c.RemoteAddress);
            }
        }

        public ChatUser() {
        }

        public ChatUser(FlashiiAuth auth) {
            UserId = auth.UserId;
            ApplyAuth(auth, true);
        }

        public void ApplyAuth(FlashiiAuth auth, bool invalidateRestrictions = false) {
            Username = auth.Username;

            if (Status == ChatUserStatus.Offline)
                Status = ChatUserStatus.Online;
            
            Colour = new ChatColour(auth.ColourRaw);
            Rank = auth.Rank;
            Permissions = auth.Permissions;

            if (invalidateRestrictions || !IsSilenced)
                SilencedUntil = auth.SilencedUntil;
        }

        public void Send(IServerPacket packet) {
            lock(Sessions)
                foreach (ChatUserSession conn in Sessions)
                    conn.Send(packet);
        }

        public void Close() {
            lock (Sessions) {
                foreach (ChatUserSession conn in Sessions)
                    conn.Dispose();
                Sessions.Clear();
            }
        }

        public void ForceChannel(ChatChannel chan = null)
            => Send(new UserChannelForceJoinPacket(chan ?? CurrentChannel));

        public void FocusChannel(ChatChannel chan) {
            lock(Channels) {
                if(InChannel(chan))
                    CurrentChannel = chan;
            }
        }

        public bool InChannel(ChatChannel chan) {
            lock (Channels)
                return Channels.Contains(chan);
        }

        public void JoinChannel(ChatChannel chan) {
            lock (Channels) {
                if(!InChannel(chan)) {
                    Channels.Add(chan);
                    CurrentChannel = chan;
                }
            }
        }

        public void LeaveChannel(ChatChannel chan) {
            lock(Channels) {
                Channels.Remove(chan);
                CurrentChannel = Channels.FirstOrDefault();
            }
        }

        public IEnumerable<ChatChannel> GetChannels() {
            lock (Channels)
                return Channels.ToList();
        }

        public void AddSession(ChatUserSession sess) {
            if (sess == null)
                return;
            sess.User = this;

            lock (Sessions)
                Sessions.Add(sess);
        }

        public void RemoveSession(ChatUserSession sess) {
            if (sess == null)
                return;
            if(!sess.IsDisposed) // this could be possible
                sess.User = null;

            lock(Sessions)
                Sessions.Remove(sess);
        }

        public IEnumerable<ChatUserSession> GetDeadSessions() {
            lock (Sessions)
                return Sessions.Where(x => x.HasTimedOut || x.IsDisposed).ToList();
        }
    }
}
