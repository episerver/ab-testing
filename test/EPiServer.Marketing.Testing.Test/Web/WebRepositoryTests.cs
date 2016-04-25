using EPiServer.Marketing.Testing;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class WebRepositoryTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<ITestManager> _testManager;

        private MarketingTestingWebRepository GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testManager = new Mock<ITestManager>();
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testManager.Object);

            var aRepo = new MarketingTestingWebRepository(_serviceLocator.Object);
            return aRepo;
        }

        [Fact]
        public void GetActiveTestForContent_gets_a_test_if_it_exists_for_the_content()
        {
            var aRepo = GetUnitUnderTest();
            _testManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest> { new ABTest() { State = Data.Enums.TestState.Active } });
            var aReturnValue = aRepo.GetActiveTestForContent(Guid.NewGuid());
            Assert.True(aReturnValue != null);
        }

        [Fact]
        public void GetActiveTestForContent_returns_empty_test_when_a_test_does_not_exist_for_the_content()
        {
            var aRepo = GetUnitUnderTest();
            _testManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());
            var aReturnValue = aRepo.GetActiveTestForContent(Guid.NewGuid());
            Assert.True(aReturnValue.Id == Guid.Empty);
        }
    }
}
