using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class WelcomeReply : Reply {
        public const int CODE = 1;

        public override int ReplyCode => CODE;

        protected override string BuildLine() {
            // todo: allow customisation
            return @":Welcome to SharpChat's IRC endpoint flash!flash@irc.railgun.sh";
        }
    }
}
