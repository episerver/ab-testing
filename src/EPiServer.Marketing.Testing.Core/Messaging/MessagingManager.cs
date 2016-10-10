using EPiServer.Marketing.Messaging;
using EPiServer.Marketing.Messaging.InMemory;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Messaging.Messages;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <summary>
    /// The messaging manager provides an interface for sending asynchronous messages to update view and 
    /// conversion data for a specific test. Since its a service it can be loaded via the standard service 
    /// locator methods found in Epi
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IMessagingManager), Lifecycle = ServiceInstanceScope.Singleton)]
    class MessagingManager : IMessagingManager
    {
        private string QueName = "TestingQueue";
        private IServiceLocator _serviceLocator;
        private MessageHandlerRegistry registry;
        private MessagingApplicationBuilder appBuilder;
        private InMemoryQueueStore _queueStore;
        private ITestingMessageHandler _handler;
        private BlockingCollection<object> _queue;

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
        internal MessagingManager(IServiceLocator locator, ITestingMessageHandler handler)
        {
            _serviceLocator = locator;
            _handler = handler;
            Init();
        }

        /// <summary>
        /// Initializes queue internals and registers the supported messages,
        /// note that the ITestingMessageHandler is retrieved from service locator
        /// so that we can mock it in tests.
        /// </summary>
        internal void Init()
        {
            registry = new MessageHandlerRegistry();
            appBuilder = new MessagingApplicationBuilder();

            // register the handler for each message
            if (_handler == null)
                _handler = new TestingMessageHandler();

            registry.Register<UpdateViewsMessage>(_handler);
            registry.Register<UpdateConversionsMessage>(_handler);
            registry.Register<AddKeyResultMessage>(_handler);

            // Create the dispatcher, queue store, and the memory reciever
            var messageDispatcher = new FanOutMessageDispatcher(registry);
            _queueStore = new InMemoryQueueStore(AppDomain.CurrentDomain);
            _queue = _queueStore.Get(QueName);
            var messageReceiver = new InMemoryMessageReceiver(messageDispatcher, _queue);

            // Initialize the message application builder
            appBuilder.Add(messageReceiver);
            appBuilder.App.Start();
        }

        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }

        /// <summary>
        /// Emits the asynchronous message to update the view result for the specified VariantId
        /// </summary>
        /// <param name="TestId">the test id to work with</param>
        /// <param name="VariantId">the Guid of the cms item that was viewed</param>
        public void EmitUpdateViews(Guid TestId, Guid VariantId, int itemVersion)
        {
            var emitterFactory = new InMemoryMessageEmitter(_queueStore.Get(QueName));
            emitterFactory.Emit<UpdateViewsMessage>(new UpdateViewsMessage() { TestId = TestId, VariantId = VariantId, ItemVersion = itemVersion});
        }

        /// <summary>
        /// Emits the asynchronous message to update a conversion result for the specified VariantId
        /// </summary>
        /// <param name="TestId"></param>
        /// <param name="VariantId">the Guid of the cms item that caused a converion</param>
        public void EmitUpdateConversion(Guid TestId, Guid VariantId, int itemVersion)
        {
            var emitterFactory = new InMemoryMessageEmitter(_queueStore.Get(QueName));
            emitterFactory.Emit<UpdateConversionsMessage>(new UpdateConversionsMessage() {TestId = TestId, VariantId=VariantId, ItemVersion = itemVersion } );
        }

        public void EmitKpiResultData(Guid testId, Guid itemId, int itemVersion, IKeyResult keyResult, KeyResultType type)
        {
            var emitterFactory = new InMemoryMessageEmitter(_queueStore.Get(QueName));
            emitterFactory.Emit(new AddKeyResultMessage()
            {
                TestId = testId,
                VariantId = itemId,
                ItemVersion = itemVersion,
                Result = keyResult,
                Type = type
            });
        }
    }
}
