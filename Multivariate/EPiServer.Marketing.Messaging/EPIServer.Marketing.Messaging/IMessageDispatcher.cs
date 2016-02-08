namespace EPiServer.Marketing.Messaging
{
    /// <summary>
    /// The IMessageDispatcher interface describes a component capable of
    /// dispatching messages according to a defined routing algorithm.
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Dispatches the specified message.
        /// </summary>
        /// <param name="message">Message to route</param>
        void Dispatch(object message);
    }
}
