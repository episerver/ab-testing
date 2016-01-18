using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Multivariate.Web.Messaging
{
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
            throw new NotImplementedException();
        }

        public void Handle(UpdateViewsMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
