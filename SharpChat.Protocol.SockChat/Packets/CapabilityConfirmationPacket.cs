using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Protocol.SockChat.Packets {
    public class CapabilityConfirmationPacket : ServerPacket {
        private IEnumerable<string> Capabilities { get; }

        private static readonly string[] Names = Enum.GetNames(typeof(ClientCapability));
        private static readonly int[] Values = Enum.GetValues(typeof(ClientCapability)) as int[];

        public CapabilityConfirmationPacket(ClientCapability caps) {
            Capabilities = GetStrings((int)caps);
        }

        private static IEnumerable<string> GetStrings(int caps) {
            for(int i = 0; i < Values.Length; ++i)
                if((caps & Values[i]) > 0)
                    yield return Names[i];
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.CapabilityConfirm);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(string.Join(' ', Capabilities));

            return sb.ToString();
        }
    }
}
