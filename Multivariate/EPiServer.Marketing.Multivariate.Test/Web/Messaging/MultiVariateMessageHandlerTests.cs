using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.Marketing.Multivariate.Web.Messaging;

namespace EPiServer.Marketing.Multivariate.Test.Web.Messaging
{
    [TestClass]
    public class MultiVariateMessageHandlerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IMultivariateTestRepository> _testrepository;

        private MultiVariateMessageHandler GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testrepository = new Mock<IMultivariateTestRepository>();

            return new MultiVariateMessageHandler(_serviceLocator.Object);
        }

        [TestMethod]
        public void HandleUpdateConversionsMessageGetsRepoFromServiceLocator()
        {
            var handler = GetUnitUnderTest();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestRepository>()).Returns(_testrepository.Object);

            handler.Handle( new UpdateConversionsMessage() );
            _serviceLocator.Verify(sl => sl.GetInstance<IMultivariateTestRepository>(), Times.Once, "GetInstance was never called");
        }

        [TestMethod]
        public void HandleUpdateConversionsMessageCallsRepoUpdateView()
        {
            var handler = GetUnitUnderTest();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestRepository>()).Returns(_testrepository.Object);

            handler.Handle(new UpdateConversionsMessage());
            _testrepository.Verify(tp => tp.UpdateConversion(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once, "MessageHandler never called repository UpdateConversion ");
        }

        [TestMethod]
        public void HandleUpdateViewMessageGetsRepoFromServiceLocator()
        {
            var handler = GetUnitUnderTest();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestRepository>()).Returns(_testrepository.Object);

            handler.Handle(new UpdateViewsMessage());
            _serviceLocator.Verify(sl => sl.GetInstance<IMultivariateTestRepository>(), Times.Once, "GetInstance was never called");
        }

        [TestMethod]
        public void HandleUpdateViewsMessageCallsRepoUpdateView()
        {
            var handler = GetUnitUnderTest();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestRepository>()).Returns(_testrepository.Object);

            handler.Handle(new UpdateViewsMessage());
            _testrepository.Verify(tp => tp.UpdateView(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once, "MessageHandler never called repository UpdateView ");
        }
    }
}
