using System;
using Moq;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.Marketing.Multivariate.Messaging;
using EPiServer.Marketing.Multivariate.Web.Controllers;
using EPiServer.Marketing.Multivariate.Model;
using Xunit;

namespace EPiServer.Marketing.Multivariate.Test.Web
{
        public class MultivariateRestStoreTest
    {
        private static Mock<IServiceLocator> _serviceLocator;
        private static Mock<IMultivariateTestManager> _testManager;
        private static Mock<IMessagingManager> _messageManager;

        private MultivariateRestStore GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testManager = new Mock<IMultivariateTestManager>();
            _messageManager = new Mock<IMessagingManager>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testManager.Object);
            _serviceLocator.Setup(sl => sl.GetInstance<IMessagingManager>()).Returns(_messageManager.Object);

            return new MultivariateRestStore(_serviceLocator.Object);
        }

        [Fact]
        public void GetCallsGetTestList_WhenIDIsNull()
        {
            var unit = GetUnitUnderTest();
            unit.Get(null);

            _testManager.Verify(tm => tm.GetTestList(It.IsAny<MultivariateTestCriteria>()), "Get did not call getTestList when id is null");
        }

        [Fact]
        public void GetCallsGetWhenIDIsValidGuid()
        {
            Guid testGuid = Guid.NewGuid();
            var unit = GetUnitUnderTest();
            unit.Get(testGuid.ToString());

            _testManager.Verify(tm => tm.Get(It.Is<Guid>( gg => gg.ToString().Equals(testGuid.ToString()))), 
                "Get did not call TestManager.Get with proper Guid");
        }

        [Fact]
        public void PostEmitsUpdateConversionWhenConversionIsNotNull()
        {
            var unit = GetUnitUnderTest();
            unit.Post(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 1);

            _messageManager.Verify(mm => mm.EmitUpdateConversion(It.IsAny<Guid>(), It.IsAny<Guid>()),
                "MessageManager did not call EmitUpdateConversion when converson argument is not null");
        }

        [Fact]
        public void PostEmitsUpdateViewWhenConversionIsNull()
        {
            var unit = GetUnitUnderTest();
            unit.Post(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null);

            _messageManager.Verify(mm => mm.EmitUpdateViews(It.IsAny<Guid>(), It.IsAny<Guid>()),
                "MessageManager did not call EmitUpdateViews when converson argument is null");
        }
    }
}
