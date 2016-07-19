using System;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;
using EPiServer.Marketing.Testing.Web.Repositories;

namespace EPiServer.Marketing.Testing.Test.Web
{
        public class TestingRestStoreTest
    {
        private static Mock<IServiceLocator> _serviceLocator;
        private static Mock<IMarketingTestingWebRepository> _webRepo;
        private static Mock<IMessagingManager> _messageManager;

        private TestingRestStore GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _webRepo = new Mock<IMarketingTestingWebRepository>();
            _messageManager = new Mock<IMessagingManager>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMarketingTestingWebRepository>()).Returns(_webRepo.Object);
            _serviceLocator.Setup(sl => sl.GetInstance<IMessagingManager>()).Returns(_messageManager.Object);

            return new TestingRestStore(_serviceLocator.Object);
        }

        [Fact]
        public void GetCallsGetTestList_WhenIDIsNull()
        {
            var unit = GetUnitUnderTest();
            unit.Get(null);

            _webRepo.Verify(tm => tm.GetTestList(It.IsAny<Testing.Data.TestCriteria>()), "Get did not call getTestList when id is null");
        }

        [Fact]
        public void GetCallsGetWhenIDIsValidGuid()
        {
            Guid testGuid = Guid.NewGuid();
            var unit = GetUnitUnderTest();
            unit.Get(testGuid.ToString());

            _webRepo.Verify(tm => tm.GetTestById(It.Is<Guid>( gg => gg.ToString().Equals(testGuid.ToString()))), 
                "Get did not call TestManager.Get with proper Guid");
        }

        [Fact]
        public void PostEmitsUpdateConversionWhenConversionIsNotNull()
        {
            var unit = GetUnitUnderTest();
            unit.Post(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 1);

            _messageManager.Verify(mm => mm.EmitUpdateConversion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()),
                "MessageManager did not call EmitUpdateConversion when converson argument is not null");
        }

        [Fact]
        public void PostEmitsUpdateViewWhenConversionIsNull()
        {
            var unit = GetUnitUnderTest();
            unit.Post(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null);

            _messageManager.Verify(mm => mm.EmitUpdateViews(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()),
                "MessageManager did not call EmitUpdateViews when converson argument is null");
        }
    }
}
