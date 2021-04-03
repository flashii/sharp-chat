namespace SharpChat.Protocol.SockChat.Packets {
    public abstract class ServerPacket : IServerPacket {
        private string Packed { get; set; }

        protected abstract string DoPack();

        public string Pack() {
            if(Packed == null)
                Packed = DoPack();
            return Packed;
        }
    }
}
