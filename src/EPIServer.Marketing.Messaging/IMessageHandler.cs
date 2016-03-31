namespace EPiServer.Marketing.Messaging
{
    /// <summary>
    /// The IMessageHandler interface describes a component capable of
    /// processing a particular type of message.
    /// </summary>
    /// <typeparam name="T">Type of message to handler</typeparam>
    public interface IMessageHandler<T>
    {
        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">Message to be processed</param>
        void Handle(T message);
    }
}
