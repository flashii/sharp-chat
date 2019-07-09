﻿using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public string Colour { get; set; }
        public int Hierarchy { get; set; }
        public string Nickname { get; set; }

        public bool IsAway { get; set; }
        public DateTimeOffset SilencedUntil { get; set; }
        public DateTimeOffset BannedUntil { get; set; }

        public bool IsModerator { get; private set; } = false;
        public bool CanChangeNick { get; private set; } = false;
        public SockChatUserChannel CanCreateChannels { get; private set; } = SockChatUserChannel.No;

        public readonly List<SockChatConn> Connections = new List<SockChatConn>();

        public SockChatChannel Channel { get; set; }

        public bool IsSilenced
            => SilencedUntil != null && DateTimeOffset.UtcNow - SilencedUntil <= TimeSpan.Zero;
        public bool IsBanned
            => BannedUntil != null && DateTimeOffset.UtcNow - BannedUntil <= TimeSpan.Zero;

        public string DisplayName
            => !string.IsNullOrEmpty(Nickname)
                ? (IsAway ? string.Empty : @"~") + Nickname
                : Username;

        public IEnumerable<string> RemoteAddresses
            => Connections.Select(c => c.Websocket.RemoteAddress());

        public SockChatUser()
        {
        }

        public SockChatUser(FlashiiAuth auth)
        {
            UserId = auth.UserId;
            ApplyAuth(auth, true);
        }

        public void ApplyAuth(FlashiiAuth auth, bool usernameAsWell = false, bool invalidateRestrictions = false)
        {
            if (usernameAsWell || IsAway)
            {
                Username = auth.Username;
                Nickname = null;
                IsAway = false;
            }

            Colour = auth.Colour;
            Hierarchy = auth.Hierarchy;
            IsModerator = auth.IsModerator;
            CanChangeNick = auth.CanChangeNick;
            CanCreateChannels = auth.CanCreateChannels;

            if(invalidateRestrictions || !IsBanned)
                BannedUntil = auth.BannedUntil;

            if(invalidateRestrictions || !IsSilenced)
                SilencedUntil = auth.SilencedUntil;
        }

        public void Send(string data)
            => Connections.ForEach(c => c.Send(data));

        public void Send(SockChatClientMessage inst, params string[] parts)
            => Send(parts.Pack(inst));

        public void Send(SockChatUser user, string message, string flags = @"10010")
        {
            user = user ?? SockChatServer.Bot;
            Send(
                SockChatClientMessage.MessageAdd,
                Utils.UnixNow, user.UserId.ToString(),
                message, SockChatMessage.NextMessageId,
                flags
            );
        }

        public void Send(bool error, string id, params string[] args)
        {
            Send(null, SockChatMessage.PackBotMessage(error ? 1 : 0, id, args));
        }

        public void Close()
        {
            lock (Connections)
            {
                Connections.ForEach(c => c.Close());
                Connections.Clear();
            }
        }

        public void ForceChannel(SockChatChannel chan = null)
            => Send(SockChatClientMessage.UserSwitch, @"2", (chan ?? Channel).ToString());

        public void AddConnection(SockChatConn conn)
            => Connections.Add(conn);

        public void AddConnection(IWebSocketConnection conn)
            => Connections.Add(new SockChatConn(conn));

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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append(Constants.SEPARATOR);
            sb.Append(DisplayName);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Colour);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Hierarchy);
            sb.Append(' ');
            sb.Append(IsModerator.AsChar());
            sb.Append(@" 0 ");
            sb.Append(CanChangeNick.AsChar());
            sb.Append(' ');
            sb.Append((int)CanCreateChannels);

            return sb.ToString();
        }
    }
}
