﻿using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat
{
    public class SockChatChannel
    {
        public string Name { get; set; }
        public string Password { get; set; } = string.Empty;
        public bool IsTemporary { get; set; } = false;
        public int Hierarchy { get; set; } = 0;
        public SockChatUser Owner { get; set; } = null;

        public readonly List<SockChatUser> Users = new List<SockChatUser>();

        public bool HasPassword
            => !string.IsNullOrEmpty(Password);

        public SockChatChannel()
        {
        }

        public SockChatChannel(string name)
        {
            Name = name;
        }

        public bool HasUser(SockChatUser user)
            => Users.Contains(user);

        public void UserJoin(SockChatUser user)
        {
            lock (Users)
            {
                if(user.Channel != this)
                {
                    user.Channel?.UserLeave(user);
                    user.Channel = this;
                }

                if(!Users.Contains(user))
                    Users.Add(user);
            }
        }

        public void UserLeave(SockChatUser user)
        {
            lock (Users)
            {
                if (user.Channel == this)
                    user.Channel = null;

                if(Users.Contains(user))
                    Users.Remove(user);
            }
        }

        public void Send(IServerPacket packet, int eventId = 0)
        {
            lock (Users)
                Users.ForEach(u => u.Send(packet, eventId));
        }

        [Obsolete(@"Use Send(IServerPacket, int)")]
        public void Send(string data)
        {
            lock(Users)
                Users.ForEach(u => u.Send(data));
        }

        [Obsolete(@"Use Send(IServerPacket, int)")]
        public void Send(SockChatUser user, string message, MessageFlags flags = MessageFlags.RegularUser)
        {
            message = new[] { DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), user.UserId.ToString(), message, SockChatMessage.NextMessageId.ToString(), flags.Serialise() }.Pack(SockChatServerPacket.MessageAdd);
            Send(message);
        }

        //[Obsolete(@"Use Send(IServerPacket, int)")]
        public void Send(bool error, string id, params string[] args)
        {
            Send(SockChatServer.Bot, SockChatMessage.PackBotMessage(error ? 1 : 0, id, args));
        }

        public void UpdateUser(SockChatUser user)
        {
            Send(new UserUpdatePacket(user));
        }

        public IEnumerable<SockChatUser> GetUsers(IEnumerable<SockChatUser> exclude = null)
        {
            lock (Users)
            {
                IEnumerable<SockChatUser> users = Users;

                if (exclude != null)
                    users = users.Except(exclude);

                return users.ToList();
            }
        }

        public string GetUsersString(IEnumerable<SockChatUser> exclude = null)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<SockChatUser> users = GetUsers(exclude);

            sb.Append(users.Count());

            foreach (SockChatUser user in users)
            {
                sb.Append('\t');
                sb.Append(user);
                sb.Append("\t1");
            }

            return sb.ToString();
        }

        public string Pack(int targetVersion = 1)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);
            sb.Append(Constants.SEPARATOR);
            sb.Append(string.IsNullOrEmpty(Password) ? '0' : '1');
            sb.Append(Constants.SEPARATOR);
            sb.Append(IsTemporary ? '1' : '0');

            return sb.ToString();
        }

        public override string ToString()
        {
            return Pack();
        }
    }
}
