namespace SharpChat.Protocol.IRC.ServerCommands {
    public abstract class ServerCommand : IServerCommand {
        public abstract string CommandName { get; }
        private string Line { get; set; }

        protected abstract string BuildLine();

        public string GetLine() {
            if(Line == null)
                Line = BuildLine();
            return Line;
        }
    }
}
