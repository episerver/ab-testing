using System;
using System.Collections.Concurrent;

namespace EPiServer.Marketing.Messaging.InMemory
{
    /// <summary>
    /// The InMemoryQueueStore class is a repository delivering
    /// in-memory queues for systems within a particular application
    /// domain.
    /// </summary>
    public class InMemoryQueueStore
    {
        private static readonly object queueLock = new object();

        private AppDomain domain;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="appDomain">Application domain from which to retrieve queues</param>
        public InMemoryQueueStore(AppDomain appDomain)
        {
            this.domain = appDomain;
        }

        /// <summary>
        /// Gets a queue with the specified name. If the queue does not
        /// exist, it will be created.
        /// </summary>
        /// <param name="queueName">Name of the queue</param>
        /// <returns>Queue with the specified name</returns>
        public BlockingCollection<object> Get(string queueName)
        {
            var queueUri = this.GenerateQueueUri(queueName);
            BlockingCollection<object> queue;

            lock (queueLock)
            {
                queue = this.GetQueue(queueUri) ?? this.CreateQueue(queueUri);
            }

            return queue;
        }

        /// <summary>
        /// Gets a queue identified by the specified URI.
        /// </summary>
        /// <param name="queueUri">URI identifying the queue</param>
        /// <returns>Queue with the specified URI</returns>
        private BlockingCollection<object> GetQueue(string queueUri)
        {
            BlockingCollection<object> queue;

            queue = (BlockingCollection<object>)domain.GetData(queueUri);

            return queue;
        }

        /// <summary>
        /// Creates a queue identified by the specified URI.
        /// </summary>
        /// <param name="queueUri">URI identifying the queue</param>
        /// <returns>Queue with the specified URI</returns>
        private BlockingCollection<object> CreateQueue(string queueUri)
        {
            BlockingCollection<object> queue;

            queue = new BlockingCollection<object>();

            domain.SetData(queueUri, queue);

            return queue;
        }

        /// <summary>
        /// Generates the URI identifying a queue with the specified
        /// name.
        /// </summary>
        /// <param name="queueName">Name of queue</param>
        /// <returns>Queue URI</returns>
        private string GenerateQueueUri(string queueName)
        {
            return "episerver:queue:" + queueName;
        }
    }
}
