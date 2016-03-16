using System.Collections.Concurrent;

namespace EPiServer.Marketing.Messaging.InMemory
{
    /// <summary>
    /// The InMemoryMessageEmitter class issues messages to an in-memory
    /// message queue.
    /// </summary>
    public class InMemoryMessageEmitter : IMessageEmitter
    {
        private readonly BlockingCollection<object> queue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queue">Queue to which messages will be issued</param>
        public InMemoryMessageEmitter(BlockingCollection<object> queue)
        {
            this.queue = queue;
        }

        /// <summary>
        /// Emits messages to the in-memory queue associated with this emitter.
        /// </summary>
        /// <typeparam name="T">Type of message to emit</typeparam>
        /// <param name="message">Message to be emitted</param>
        public void Emit<T>(T message)
        {
            this.queue.Add(message);
        }
    }
}
