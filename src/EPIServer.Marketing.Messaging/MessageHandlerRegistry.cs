using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Marketing.Messaging
{
    /// <summary>
    /// The MessageHandlerRegistry class is a repository delivering
    /// the message handlers configured for processing messages
    /// received by a system.
    /// </summary>
    public class MessageHandlerRegistry
    {
        private Dictionary<Type, List<object>> handlers;

        /// <summary>
        /// Constructor
        /// </summary>
        public MessageHandlerRegistry()
        {
            this.handlers = new Dictionary<Type, List<object>>();
        }
        
        /// <summary>
        /// Registers a message handler capable of processing a particular
        /// type of message.
        /// </summary>
        /// <typeparam name="T">Type of message processed by the specified handler</typeparam>
        /// <param name="handler">Handler to be registered</param>
        public void Register<T>(IMessageHandler<T> handler)
        {
            List<object> handlers = this.FindOrCreateHandlerCollection(typeof(T));
            handlers.Add(handler);
        }

        /// <summary>
        /// Registers a message handler capable of processing a particular
        /// type of message.
        /// </summary>
        /// <param name="type">Type of message processed by the specified handler</param>
        /// <param name="handler">Handler to be registered</param>
        public void Register(Type type, object handler)
        {
            List<object> handlers = this.FindOrCreateHandlerCollection(type);
            handlers.Add(handler);
        }

        /// <summary>
        /// Gets a collection of message handlers capable of processing messages
        /// of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of message to be processed</typeparam>
        /// <returns>Collection of message handlers capable of processing messages of the specified type</returns>
        public IEnumerable<IMessageHandler<T>> Get<T>()
        {
            return this.FindOrCreateHandlerCollection(typeof(T)).Select(handler => (IMessageHandler<T>)handler);
        }

        /// <summary>
        /// Gets a collection of message handlers capable of processing messages
        /// of the specified type.
        /// </summary>
        /// <param name="type">Type of message to be processed</param>
        /// <returns>Collection of message handlers capable of processing messages of the specified type</returns>
        public IEnumerable<object> Get(Type type)
        {
            return this.FindOrCreateHandlerCollection(type);
        }

        /// <summary>
        /// Gets a collection of message handlers registered to process messages
        /// of the specified type. If no handlers have been registered for the
        /// specified message type, an empty handler collection will be returned.
        /// </summary>
        /// <param name="key">Type of message to be proccessed</param>
        /// <returns>Collection of message handlers capable of processing messages of the specified type</returns>
        private List<object> FindOrCreateHandlerCollection(Type key)
        {
            if (!this.handlers.ContainsKey(key))
            {
                this.handlers.Add(key, new List<object>());
            }

            return this.handlers[key];
        }
    }
}
