using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using System.Threading;
using EPiServer.Marketing.Multivariate.Messaging;

namespace EPiServer.Marketing.Multivariate.Test.Messaging
{
    [TestClass]
    public class MessagingManagerTests
    {
        private static Mock<IServiceLocator> _serviceLocator;
        private static Mock<IMultivariateTestRepository> _testRepository;
        private static Mock<IMultiVariateMessageHandler> _messageHandler;

        private MessagingManager GetUnitUnderTest()
        {
            if (_serviceLocator == null)
            {
                _serviceLocator = new Mock<IServiceLocator>();
                _testRepository = new Mock<IMultivariateTestRepository>();
                _messageHandler = new Mock<IMultiVariateMessageHandler>();
                _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestRepository>()).Returns(_testRepository.Object);
                _serviceLocator.Setup(sl => sl.GetInstance<IMultiVariateMessageHandler>()).Returns(_messageHandler.Object);
            }

            return new MessagingManager(_serviceLocator.Object);
        }

        [TestMethod]
        public void EmitUpdateViewsEmitsMessageAndCallsMessageHandler()
        {
            var messageManager = GetUnitUnderTest();
            messageManager.EmitUpdateViews(Guid.Empty, Guid.NewGuid());
            Thread.Sleep(1000);

            _messageHandler.Verify(mh => mh.Handle(It.IsAny<UpdateViewsMessage>()),
                Times.AtLeastOnce, "MessageManager did not emit message or did not call handle for EmitUpdateViews");
        }

        [TestMethod]
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
