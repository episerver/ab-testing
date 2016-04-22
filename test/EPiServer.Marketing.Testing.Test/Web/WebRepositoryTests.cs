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
        public void IsContentUnderTest_is_true_when_a_test_exists_for_the_content()
        {
            var aRepo = GetUnitUnderTest();
            _testManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest> { new ABTest() });
            var aReturnValue = aRepo.IsContentUnderTest(Guid.NewGuid());
            Assert.True(aReturnValue);
        }

        [Fact]
        public void IsContentUnderTest_is_false_when_a_test_does_not_exist_for_the_content()
        {
            var aRepo = GetUnitUnderTest();
            _testManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());
            var aReturnValue = aRepo.IsContentUnderTest(Guid.NewGuid());
            Assert.False(aReturnValue);
        }
    }
}
