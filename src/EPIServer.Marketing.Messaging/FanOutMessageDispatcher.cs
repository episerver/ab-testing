namespace EPiServer.Marketing.Messaging
{
    /// <summary>
    /// The FanOutMessageDispatcher class routes messages to all registered 
    /// message handlers capable of processing messages of a particular type.
    /// </summary>
    public class FanOutMessageDispatcher : IMessageDispatcher
    {
        private readonly MessageHandlerRegistry handlerRegistry;
        private readonly MessageHandlerInvoker handlerInvoker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlerRegistry">Collection of registered message handlers</param>
        public FanOutMessageDispatcher(MessageHandlerRegistry handlerRegistry)
        {
            this.handlerRegistry = handlerRegistry;
            this.handlerInvoker = new MessageHandlerInvoker();
        }

        /// <summary>
        /// Dispatches the specified message to all registered handlers capable
        /// of processing messages of this type of message.
        /// </summary>
        /// <param name="message">Message to be dispatched</param>
        public void Dispatch(object message)
        {
            var handlers = this.handlerRegistry.Get(message.GetType());

            foreach (var handler in handlers)
            {
                this.HandleMessage(handler, message);
            }
        }

        /// <summary>
        /// Directs the specified handler to process a message.
        /// </summary>
        /// <param name="handler">Handler to process the message</param>
        /// <param name="message">Message to be processed</param>
        private void HandleMessage(object handler, object message)
        {
            try
            {
                this.handlerInvoker.Invoke(handler, message);
            }
            catch
            {
                
            }
        }
    }
}
