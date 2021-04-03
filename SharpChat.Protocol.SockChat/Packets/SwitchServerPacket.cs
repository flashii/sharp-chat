using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Protocol.SockChat.Packets {
    public class SwitchServerPacket : ServerPacket {
        public SwitchServerPacket() {
            // definition unfinished
            // optional argument containing server, if no argument assume client has a list?
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.SwitchServer);

            return sb.ToString();
        }
    }
}
