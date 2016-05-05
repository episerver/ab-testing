using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Jobs;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class JobTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<ITestManager> _testManager = new Mock<ITestManager>();
        private Guid TestToStart = Guid.NewGuid();
        private Guid TestToStop = Guid.NewGuid();

        private TestSchedulingJob GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testManager.Object);

            var list = new List<IMarketingTest>()
            {
                new ABTest() { Id = TestToStart,
                    StartDate = DateTime.Now.ToUniversalTime().AddHours(-1),
                    State = Data.Enums.TestState.Inactive },
                new ABTest() { Id = TestToStop,
                    EndDate = DateTime.Now.ToUniversalTime().AddHours(-1),
                    State = Data.Enums.TestState.Active },
                new ABTest() { Id = Guid.NewGuid(),
                    EndDate = DateTime.Now.ToUniversalTime().AddHours(-1),
                    State = Data.Enums.TestState.Done },
                new ABTest() { Id = Guid.NewGuid(),
                    EndDate = DateTime.Now.ToUniversalTime().AddHours(-1),
                    State = Data.Enums.TestState.Archived },
            };

            _testManager.Setup(m => m.GetTestList(It.IsAny<TestCriteria>())).Returns(list);

            return new TestSchedulingJob(_locator.Object);
        }

        [Fact]
        public void ExcuteStartsTest()
        {
            var unit = GetUnitUnderTest();
            unit.Execute();

            _testManager.Verify(tm => tm.GetTestList(It.IsAny<Testing.Data.TestCriteria>()), 
                "Get did not call getTestList");
            _testManager.Verify(tm => tm.Start(It.Is<Guid>(g => g == TestToStart)), Times.Once,
                "Failed to start test with proper Guid");
        }

        [Fact]
        public void ExcuteStopsTest()
        {
            var unit = GetUnitUnderTest();
            unit.Execute();

            _testManager.Verify(tm => tm.GetTestList(It.IsAny<Testing.Data.TestCriteria>()), 
                "Get did not call getTestList");
            _testManager.Verify(tm => tm.Stop(It.Is<Guid>(g => g == TestToStop)), Times.Once, 
                "Failed to stop test with proper Guid");
        }
    }
}
