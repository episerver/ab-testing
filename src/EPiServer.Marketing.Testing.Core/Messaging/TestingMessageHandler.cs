using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Core.Messaging.Messages;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <summary>
    /// The message handler simply handles the messages and passes them on the registered
    /// ITestingMessageHandler which in turn handles the cache and database layer.
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
            tm.IncrementCount(message.TestId, message.ItemVersion, CountType.View, false);
        }

        public void Handle(UpdateConversionsMessage message)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();
            tm.IncrementCount(message.TestId, message.ItemVersion, CountType.Conversion, false);
        }

        public void Handle(AddKeyResultMessage message)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();
            tm.AddKpiResultData(message.TestId, message.ItemVersion, message.Result, message.Type);
        }
    }
}
