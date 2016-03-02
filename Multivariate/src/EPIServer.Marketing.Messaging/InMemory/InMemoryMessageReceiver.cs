using System;
using System.Collections.Concurrent;
using System.Threading;

namespace EPiServer.Marketing.Messaging.InMemory
{
    /// <summary>
    /// The InMemoryMessageReceiver class receives messages that have
    /// been emitted to an in-memory message queue.
    /// </summary>
    public class InMemoryMessageReceiver : IMessageReceiver
    {
        private readonly IMessageDispatcher dispatcher;
        private readonly BlockingCollection<object> queue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dispatcher">Dispatching algorithm by which messages should be handled</param>
        /// <param name="queue">Queue by which messages will be received</param>
        public InMemoryMessageReceiver(IMessageDispatcher dispatcher, BlockingCollection<object> queue)
        {
            this.dispatcher = dispatcher;
            this.queue = queue;
        }

        /// <summary>
        /// Directs this receiver to begin processing messages.
        /// </summary>
        /// <param name="cancellationToken">Notifies the receiver that its activities should be terminated</param>
        public void Start(CancellationToken cancellationToken)
        {
            while (this.IsRunning(ref cancellationToken))
            {
                this.DequeueMessage(ref cancellationToken, this.dispatcher.Dispatch);
            }
        }

        /// <summary>
        /// Returns true if this receiver is processing messages, false otherwise.
        /// </summary>
        /// <param name="cancellationToken">Notifies the receiver that its activities should be terminated</param>
        /// <returns>True if this receiver is processing messages, false otherwise</returns>
        private bool IsRunning(ref CancellationToken cancellationToken)
        {
            return !cancellationToken.IsCancellationRequested;
        }

        /// <summary>
        /// Dequeues the next message in the receiver's queue and provides it as
        /// input to the specified callback. If the queue is empty, this method
        /// will block execution until one becomes available.
        /// </summary>
        /// <param name="cancellationToken">Notifies the receiver that its activities should be terminated</param>
        /// <param name="callback">Action to take on the dequeued message</param>
        private void DequeueMessage(ref CancellationToken token, Action<object> callback)
        {
            object message;

            if (this.queue.TryTake(out message, 5000, token))
            {
                callback(message);
            }
        }
    }
}
