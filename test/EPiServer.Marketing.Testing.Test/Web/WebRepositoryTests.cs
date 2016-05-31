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

        [Fact]
        public void DeleteTestForContent_calls_delete_for_every_test_associated_with_the_content_guid()
        {
            var aRepo = GetUnitUnderTest();
            var testList = new List<IMarketingTest>();

            testList.Add(new ABTest() { Id = Guid.NewGuid() });
            testList.Add(new ABTest() { Id = Guid.NewGuid() });
            testList.Add(new ABTest() { Id = Guid.NewGuid() });

            _testManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(testList);
            aRepo.DeleteTestForContent(Guid.NewGuid());

            _testManager.Verify(tm => tm.Delete(It.IsAny<Guid>()), Times.Exactly(testList.Count), "Delete was not called on all the tests in the list");
        }

        [Fact]
        public void DeleteTestForContent_handles_guids_with_no_tests_associated_with_it_gracefully()
        {
            var aRepo = GetUnitUnderTest();
            var testList = new List<IMarketingTest>();

            _testManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(testList);
            aRepo.DeleteTestForContent(Guid.NewGuid());

            _testManager.Verify(tm => tm.Delete(It.IsAny<Guid>()), Times.Never, "Delete was called when it should not have been");
        }
    }
}
