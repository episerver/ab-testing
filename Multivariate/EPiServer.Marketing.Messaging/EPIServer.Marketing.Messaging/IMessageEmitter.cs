namespace EPiServer.Marketing.Messaging
{
    /// <summary>
    /// The IMessageEmitter interface describes a component issuing
    /// messages to receiving systems.
    /// </summary>
    public interface IMessageEmitter
    {
        /// <summary>
        /// Emits a message to receiving systems.
        /// </summary>
        /// <typeparam name="T">Type of message to be emitted</typeparam>
        /// <param name="message">Message to emit</param>
        void Emit<T>(T message);
    }
}
