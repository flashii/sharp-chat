﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat
{
    public class ChatChannel
    {
        public string Name { get; set; }
        public string Password { get; set; } = string.Empty;
        public bool IsTemporary { get; set; } = false;
        public int Hierarchy { get; set; } = 0;
        public ChatUser Owner { get; set; } = null;

        public readonly List<ChatUser> Users = new List<ChatUser>();

        public bool HasPassword
            => !string.IsNullOrEmpty(Password);

        public ChatChannel()
        {
        }

        public ChatChannel(string name)
        {
            Name = name;
        }

        public bool HasUser(ChatUser user)
            => Users.Contains(user);

        public void UserJoin(ChatUser user)
        {
            lock (Users)
            {
                lock (user.Channels)
                    if (!user.Channels.Contains(this))
                    {
                        user.Channel?.UserLeave(user);
                        user.Channels.Add(this);
                    }

                if(!Users.Contains(user))
                    Users.Add(user);
            }
        }

        public void UserLeave(ChatUser user)
        {
            lock (Users)
            {
                lock(user.Channels)
                    if (user.Channels.Contains(this))
                        user.Channels.Remove(this);

                if(Users.Contains(user))
                    Users.Remove(user);
            }
        }

        public void Send(IServerPacket packet)
        {
            lock (Users)
                Users.ForEach(u => u.Send(packet));
        }

        public IEnumerable<ChatUser> GetUsers(IEnumerable<ChatUser> exclude = null)
        {
            lock (Users)
            {
                IEnumerable<ChatUser> users = Users.OrderByDescending(x => x.Hierarchy);

                if (exclude != null)
                    users = users.Except(exclude);

                return users.ToList();
            }
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public string Pack(int targetVersion = 1)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);
            sb.Append('\t');
            sb.Append(string.IsNullOrEmpty(Password) ? '0' : '1');
            sb.Append('\t');
            sb.Append(IsTemporary ? '1' : '0');

            return sb.ToString();
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
