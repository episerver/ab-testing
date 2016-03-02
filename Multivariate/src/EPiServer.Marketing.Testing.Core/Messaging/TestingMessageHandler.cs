using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <summary>
    /// The message handler simply handles the messages and passes them on the registered
    /// IMultivariateTestRepository which in turn handles the cache and database layer.
    /// </summary>
    class TestingMessageHandler : ITestingMessageHandler
    {
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        public TestingMessageHandler()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// Used specifically for unit tests.
        /// </summary>
        /// <param name="locator"></param>
        internal TestingMessageHandler(IServiceLocator locator)
        {
            _serviceLocator = locator;
        }

        public void Handle(UpdateViewsMessage message)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();
            tm.IncrementCount(message.TestId, message.VariantId, CountType.View);
        }

        public void Handle(UpdateConversionsMessage message)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();
            tm.IncrementCount(message.TestId, message.VariantId, CountType.Conversion);
        }
    }
}