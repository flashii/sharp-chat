﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            lock (collection)
                foreach (T item in collection)
                    action(item);
        }

        public static string GetSignedHash(this string str, string key = null)
        {
            if (key == null)
                key = Utils.ReadFileOrDefault(@"login_key.txt", @"woomy");

            StringBuilder sb = new StringBuilder();

            using (HMACSHA256 algo = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hash = algo.ComputeHash(Encoding.UTF8.GetBytes(str));

                foreach (byte b in hash)
                    sb.AppendFormat(@"{0:x2}", b);
            }

            return sb.ToString();
        }

        public static string Serialise(this SockChatMessageFlags flags)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(flags.HasFlag(SockChatMessageFlags.Bold) ? '1' : '0');
            sb.Append(flags.HasFlag(SockChatMessageFlags.Cursive) ? '1' : '0');
            sb.Append(flags.HasFlag(SockChatMessageFlags.Underline) ? '1' : '0');
            sb.Append(flags.HasFlag(SockChatMessageFlags.Colon) ? '1' : '0');
            sb.Append(flags.HasFlag(SockChatMessageFlags.Private) ? '1' : '0');

            return sb.ToString();
        }
    }
}
