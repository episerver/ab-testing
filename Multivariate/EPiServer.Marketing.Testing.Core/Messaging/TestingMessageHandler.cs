using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing;
using EPiServer.Marketing.Testing.Messaging;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <summary>
    /// The message handler simply handles the messages and passes them on the registered
    /// IMultivariateTestRepository which in turn handles the cache and database layer.
    /// </summary>
    class TestingMessageHandler : ITestingMessageHandler
    {
        private IServiceLocator _serviceLocator;
        internal ITestManager _testManager;

        [ExcludeFromCodeCoverage]
        public TestingMessageHandler()
        {
            _serviceLocator = ServiceLocator.Current;
            _testManager = _serviceLocator.GetInstance<ITestManager>();
        }

        /// <summary>
        /// Used specifically for unit tests.
        /// </summary>
        /// <param name="locator"></param>
        internal TestingMessageHandler(IServiceLocator locator)
        {
            _serviceLocator = locator;
            _testManager = _serviceLocator.GetInstance<ITestManager>();
        }

        public void Handle(UpdateViewsMessage message)
        {
            _testManager.IncrementCount(message.TestId, message.VariantId, message.ItemVersion, CountType.View);
        }

        public void Handle(UpdateConversionsMessage message)
        {
            _testManager.IncrementCount(message.TestId, message.VariantId, message.ItemVersion, CountType.Conversion);
        }
    }
}
