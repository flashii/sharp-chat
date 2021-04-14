using SharpChat.Reflection;

namespace SharpChat.Protocol {
    public class ServerAttribute : ObjectConstructorAttribute {
        public ServerAttribute(string name) : base(name) {
        }
    }
}
