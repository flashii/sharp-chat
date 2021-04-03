using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.IRC.Replies {
    public class WelcomeReply : ServerReply {
        public override int ReplyCode => 1;

        protected override string BuildLine() {
            return @":Welcome to SharpChat's IRC endpoint flash!flash@irc.railgun.sh";
        }
    }
}
