using System;
using Moq;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using System.Threading;
using EPiServer.Marketing.Testing.Messaging;
using Xunit;

namespace EPiServer.Marketing.Multivariate.Test.Messaging
{
        public class MessagingManagerTests
    {
        private static Mock<IServiceLocator> _serviceLocator;
        private static Mock<ITestRepository> _testRepository;
        private static Mock<IMultiVariateMessageHandler> _messageHandler;

        private MessagingManager GetUnitUnderTest()
        {
            if (_serviceLocator == null)
            {
                _serviceLocator = new Mock<IServiceLocator>();
                _testRepository = new Mock<ITestRepository>();
                _messageHandler = new Mock<IMultiVariateMessageHandler>();
                _serviceLocator.Setup(sl => sl.GetInstance<ITestRepository>()).Returns(_testRepository.Object);
            }

            return new MessagingManager(_serviceLocator.Object, _messageHandler.Object);
        }

        [Fact]
        public void EmitUpdateViewsEmitsMessageAndCallsMessageHandler()
        {
            var messageManager = GetUnitUnderTest();
            messageManager.EmitUpdateViews(Guid.Empty, Guid.NewGuid());
            Thread.Sleep(1000);

            _messageHandler.Verify(mh => mh.Handle(It.IsAny<UpdateViewsMessage>()),
                Times.AtLeastOnce, "MessageManager did not emit message or did not call handle for EmitUpdateViews");
        }

        [Fact]
        public void EmitUpdateConversionEmitsMessageAndCallsMessageHandler()
        {
            var messageManager = GetUnitUnderTest();
            messageManager.EmitUpdateConversion(Guid.Empty, Guid.NewGuid());
            Thread.Sleep(1000);
            _messageHandler.Verify(mh => mh.Handle(It.IsAny<UpdateConversionsMessage>()),
                Times.AtLeastOnce, "MessageManager did not emit message or did not call handle for UpdateConversionsMessage");
        }
    }
}
