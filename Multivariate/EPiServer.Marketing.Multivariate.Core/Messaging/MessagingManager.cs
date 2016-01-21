using EPiServer.Marketing.Messaging;
using EPiServer.Marketing.Messaging.InMemory;
using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Multivariate.Messaging
{
    /// <summary>
    /// The messaging manager provides an interface for sending asynchronous messages to update view and 
    /// conversion data for a specific test. Since its a service it can be loaded via the standard service 
    /// locator methods found in Epi
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IMessagingManager))]
    class MessagingManager : IMessagingManager
    {
        private string QueName = "MultiVariantQueue";
        private IServiceLocator _serviceLocator;
        private MessageHandlerRegistry registry;
        private MessagingApplicationBuilder appBuilder;
        private InMemoryQueueStore _queueStore;

        [ExcludeFromCodeCoverage]
        public MessagingManager()
        {
            _serviceLocator = ServiceLocator.Current;
            Init();
        }

        /// <summary>
        /// Used specifically for unit tests.
        /// </summary>
        /// <param name="locator"></param>
        internal MessagingManager(IServiceLocator locator)
        {
            _serviceLocator = locator;

            Init();
        }

        /// <summary>
        /// Initializes queue internals and registers the supported messages,
        /// note that the IMultiVariateMessageHandler is retrieved from service locator
        /// so that we can mock it in tests.
        /// </summary>
        internal void Init()
        {
            registry = new MessageHandlerRegistry();
            appBuilder = new MessagingApplicationBuilder();

            // register the handler for each message
            var handler = _serviceLocator.GetInstance<IMultiVariateMessageHandler>();
            registry.Register<UpdateViewsMessage>(handler);
            registry.Register<UpdateConversionsMessage>(handler);
            
            // Create the dispatcher, queue store, and the memory reciever
            var messageDispatcher = new FanOutMessageDispatcher(registry);
            _queueStore = new InMemoryQueueStore(AppDomain.CurrentDomain);
            var queue = _queueStore.Get( QueName );
            var messageReceiver = new InMemoryMessageReceiver(messageDispatcher, queue);

            // Initialize the message application builder
            appBuilder.Add(messageReceiver);
            appBuilder.App.Start();
        }

        /// <summary>
        /// Emits the asynchronous message to update the view result for the specified VariantId
        /// </summary>
        /// <param name="TestId">the test id to work with</param>
        /// <param name="VariantId">the Guid of the cms item that was viewed</param>
        public void EmitUpdateViews(Guid TestId, Guid VariantId)
        {
            var emitterFactory = new InMemoryMessageEmitter(_queueStore.Get(QueName));
            emitterFactory.Emit<UpdateViewsMessage>(new UpdateViewsMessage() { TestId = TestId, VariantId = VariantId});
        }

        /// <summary>
        /// Emits the asynchronous message to update a conversion result for the specified VariantId
        /// </summary>
        /// <param name="TestId"></param>
        /// <param name="VariantId">the Guid of the cms item that caused a converion</param>
        public void EmitUpdateConversion(Guid TestId, Guid VariantId)
        {
            var emitterFactory = new InMemoryMessageEmitter(_queueStore.Get(QueName));
            emitterFactory.Emit<UpdateConversionsMessage>(new UpdateConversionsMessage() {TestId = TestId, VariantId=VariantId } );
        }
    }
}
