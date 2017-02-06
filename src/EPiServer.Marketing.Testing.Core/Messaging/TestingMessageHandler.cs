using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Messaging.Messages;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <inheritdoc />
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Handle(UpdateViewsMessage message)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();
            tm.IncrementCount(message.TestId, message.ItemVersion, CountType.View, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Handle(UpdateConversionsMessage message)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();
            tm.IncrementCount(message.TestId, message.ItemVersion, CountType.Conversion, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Handle(AddKeyResultMessage message)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();
            tm.SaveKpiResultData(message.TestId, message.ItemVersion, message.Result, message.Type, false);
        }
    }
}
