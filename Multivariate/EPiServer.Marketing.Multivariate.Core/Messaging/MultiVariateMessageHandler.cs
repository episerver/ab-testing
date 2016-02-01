using EPiServer.Marketing.Multivariate.Model.Enums;
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
    class MultiVariateMessageHandler : IMultiVariateMessageHandler
    {
        private IServiceLocator _serviceLocator;
        internal IMultivariateTestManager _testManager;

        [ExcludeFromCodeCoverage]
        public MultiVariateMessageHandler()
        {
            _serviceLocator = ServiceLocator.Current;
            _testManager = _serviceLocator.GetInstance<IMultivariateTestManager>();
        }

        /// <summary>
        /// Used specifically for unit tests.
        /// </summary>
        /// <param name="locator"></param>
        internal MultiVariateMessageHandler(IServiceLocator locator)
        {
            _serviceLocator = locator;
            _testManager = _serviceLocator.GetInstance<IMultivariateTestManager>();
        }

        public void Handle(UpdateViewsMessage message)
        {
            _testManager.IncrementCount(message.TestId, message.VariantId, CountType.View);
        }

        public void Handle(UpdateConversionsMessage message)
        {
            _testManager.IncrementCount(message.TestId, message.VariantId, CountType.Conversion);
        }
    }
}
