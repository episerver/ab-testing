using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Multivariate.Web.Messaging
{
    /// <summary>
    /// The message handler simply handles the messages and passes them on the registered
    /// IMultivariateTestRepository which in turn handles the cache and database layer.
    /// </summary>
    class MultiVariateMessageHandler : IMultiVariateMessageHandler
    {
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        public MultiVariateMessageHandler()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// Used specifically for unit tests.
        /// </summary>
        /// <param name="locator"></param>
        internal MultiVariateMessageHandler(IServiceLocator locator)
        {
            _serviceLocator = locator;
        }

        public void Handle(UpdateConversionsMessage message)
        {
            var repo = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            repo.UpdateConversion(message.TestId, message.VariantId);
        }

        public void Handle(UpdateViewsMessage message)
        {
            var repo = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            repo.UpdateView(message.TestId, message.VariantId);
        }
    }
}
