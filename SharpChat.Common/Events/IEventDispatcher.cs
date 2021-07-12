namespace SharpChat.Events {
    public interface IEventDispatcher {
        void AddEventHandler(IEventHandler handler);
        void RemoveEventHandler(IEventHandler handler);
        void DispatchEvent(object sender, IEvent evt);
    }
}
