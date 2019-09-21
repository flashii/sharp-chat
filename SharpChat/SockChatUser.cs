﻿using Fleck;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpChat
{
    public enum SockChatUserChannel
    {
        No = 0,
        OnlyTemporary = 1,
        Yes = 2,
    }

    public class SockChatUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public FlashiiColour Colour { get; set; }
        public int Hierarchy { get; set; }
        public string Nickname { get; set; }

        public bool IsAway => !string.IsNullOrWhiteSpace(AwayMessage);
        public string AwayMessage { get; set; }

        public DateTimeOffset SilencedUntil { get; set; }
        public DateTimeOffset BannedUntil { get; set; }

        public bool IsModerator { get; private set; } = false;
        public bool CanChangeNick { get; private set; } = false;
        public SockChatUserChannel CanCreateChannels { get; private set; } = SockChatUserChannel.No;

        public readonly List<SockChatConn> Connections = new List<SockChatConn>();

        public SockChatChannel Channel {
            get
            {
                lock (Channels)
                    return Channels.FirstOrDefault();
            }
        }

        public readonly ChatRateLimiter RateLimiter = new ChatRateLimiter();

        public readonly List<SockChatChannel> Channels = new List<SockChatChannel>();

        public bool IsSilenced
            => SilencedUntil != null && DateTimeOffset.UtcNow - SilencedUntil <= TimeSpan.Zero;
        public bool IsBanned
            => BannedUntil != null && DateTimeOffset.UtcNow - BannedUntil <= TimeSpan.Zero;
        public bool IsAlive
            => Connections.Where(c => !c.HasTimedOut).Any();

        [Obsolete(@"Use GetDisplayName instead")]
        public string DisplayName
            => !string.IsNullOrEmpty(Nickname)
                ? (IsAway ? string.Empty : @"~") + Nickname
                : Username;

        public IEnumerable<IPAddress> RemoteAddresses
            => Connections.Select(c => c.RemoteAddress);

        public SockChatUser()
        {
        }

        public SockChatUser(FlashiiAuth auth)
        {
            UserId = auth.UserId;
            ApplyAuth(auth, true);
        }

        public string GetDisplayName(int version, bool forceOriginal = false)
        {
            StringBuilder sb = new StringBuilder();

            if(version < 2 && IsAway)
                sb.AppendFormat(@"&lt;{0}&gt;_", AwayMessage.Substring(0, Math.Min(AwayMessage.Length, 5)).ToUpperInvariant());

            if (forceOriginal || string.IsNullOrWhiteSpace(Nickname))
                sb.Append(Username);
            else
            {
                if (version < 2)
                    sb.Append('~');

                sb.Append(Nickname);
            }

            return sb.ToString();
        }

        public void ApplyAuth(FlashiiAuth auth, bool invalidateRestrictions = false)
        {
            Username = auth.Username;

            if (IsAway)
            {
                Nickname = null;
                AwayMessage = null;
            }

            Colour = new FlashiiColour(auth.ColourRaw);
            Hierarchy = auth.Hierarchy;
            IsModerator = auth.IsModerator;
            CanChangeNick = auth.CanChangeNick;
            CanCreateChannels = auth.CanCreateChannels;

            if(invalidateRestrictions || !IsBanned)
                BannedUntil = auth.BannedUntil;

            if(invalidateRestrictions || !IsSilenced)
                SilencedUntil = auth.SilencedUntil;
        }

        public void Send(IServerPacket packet)
        {
            lock(Connections)
                Connections.ForEach(c => c.Send(packet));
        }

        [Obsolete(@"Use Send(IServerPacket, int)")]
        public void Send(SockChatUser user, string message, SockChatMessageFlags flags = SockChatMessageFlags.RegularUser)
        {
            user = user ?? SockChatServer.Bot;

            StringBuilder sb = new StringBuilder();
            sb.Append((int)SockChatServerPacket.MessageAdd);
            sb.Append('\t');
            sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
            sb.Append('\t');
            sb.Append(user.UserId);
            sb.Append('\t');
            sb.Append(message);
            sb.Append('\t');
            sb.Append(ServerPacket.NextSequenceId());
            sb.Append('\t');
            sb.Append(flags.Serialise());

            string packet = sb.ToString();

            Connections.ForEach(c => c.Send(packet));
        }

        [Obsolete(@"Use Send(IServerPacket, int)")]
        public void Send(bool error, string id, params string[] args)
        {
            Send(SockChatServer.Bot, SockChatMessage.PackBotMessage(error ? 1 : 0, id, args));
        }

        public void Close()
        {
            lock (Connections)
            {
                Connections.ForEach(c => c.Dispose());
                Connections.Clear();
            }
        }

        public void ForceChannel(SockChatChannel chan = null)
            => Send(new UserChannelForceJoinPacket(chan ?? Channel));

        public void AddConnection(SockChatConn conn)
            => Connections.Add(conn);

        public void RemoveConnection(SockChatConn conn)
           => Connections.Remove(conn);

        public void RemoveConnection(IWebSocketConnection conn)
            => Connections.Remove(Connections.FirstOrDefault(x => x.Websocket == conn));

        public bool HasConnection(SockChatConn conn)
            => Connections.Contains(conn);

        public bool HasConnection(IWebSocketConnection ws)
            => Connections.Any(x => x.Websocket == ws);

        public SockChatConn GetConnection(IWebSocketConnection ws)
            => Connections.FirstOrDefault(x => x.Websocket == ws);

        public string Pack(int targetVersion = 1)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append('\t');
            sb.Append(GetDisplayName(targetVersion));
            sb.Append('\t');
            if (targetVersion >= 2)
                sb.Append(Colour.Raw);
            else
                sb.Append(Colour);
            sb.Append('\t');
            sb.Append(Hierarchy);
            sb.Append(' ');
            sb.Append(IsModerator ? '1' : '0');
            sb.Append(@" 0 ");
            sb.Append(CanChangeNick ? '1' : '0');
            sb.Append(' ');
            sb.Append((int)CanCreateChannels);

            return sb.ToString();
        }
    }
}
