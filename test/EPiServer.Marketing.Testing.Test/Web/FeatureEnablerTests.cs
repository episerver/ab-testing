using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web;
using EPiServer.ServiceLocation;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class FeatureEnablerTests
    {
        private Mock<ITestHandler> mockTestHandler;
        private Mock<ITestManager> mockTestManager;
        private IMarketingTestingEvents marketingTestingEvents;
        private Mock<IServiceLocator> mockServiceLocator;

        [Fact]
        public void OnTestAddedToCache_Enables_ABTesting_If_ActiveTestCount_Is_One()
        {
            var featureEnabler = GetUnitUnderTest();
            mockTestManager.Setup(t => t.GetActiveTests()).Returns(new List<IMarketingTest> { new ABTest { State = TestState.Active } });
            mockTestHandler.Setup(t => t.EnableABTesting()).Verifiable();

            featureEnabler.TestAddedToCache(null, null);
            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Once);
        }

        [Fact]
        public void OnTestAddedToCache_DoesNotEnable_ABTesting_If_ActiveTestCount_Is_More_Than_One()
        {
            var featureEnabler = GetUnitUnderTest();
            var tests = new List<IMarketingTest> { new ABTest { State = TestState.Active }, new ABTest { State = TestState.Active } };
            mockTestManager.Setup(t => t.GetActiveTests()).Returns(tests);
            mockTestHandler.Setup(t => t.EnableABTesting()).Verifiable();

            featureEnabler.TestAddedToCache(null, null);
            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Never);
        }

        [Fact]
        public void OnTestRemovedFromCache_Disables_ABTesting_If_ActiveTestCount_Is_Zero()
        {
            var featureEnabler = GetUnitUnderTest();
            mockTestManager.Setup(t => t.GetActiveTests()).Returns(new List<IMarketingTest> { });
            mockTestHandler.Setup(t => t.DisableABTesting()).Verifiable();

            featureEnabler.TestRemovedFromCache(null, null);
            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Once);
        }

        [Fact]
        public void OnTestRemovedFromCache_DoesNotDisable_ABTesting_If_ActiveTestCount_Is_One_Or_More()
        {
            var featureEnabler = GetUnitUnderTest();
            mockTestManager.Setup(t => t.GetActiveTests()).Returns(new List<IMarketingTest> { new ABTest { State = TestState.Active } });
            mockTestHandler.Setup(t => t.DisableABTesting()).Verifiable();

            featureEnabler.TestRemovedFromCache(null, null);
            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Never);
        }

        [Fact]
        public void Constructor_Adds_EventHandlers()
        {
            GetUnitUnderTest();
            Assert.Equal(1, (marketingTestingEvents as FakeMarketingTestingEvents).TestAddedToCacheCounter);
            Assert.Equal(1, (marketingTestingEvents as FakeMarketingTestingEvents).TestRemovedFromCacheCounter);
        }

        private FeatureEnabler GetUnitUnderTest()
        {
            mockTestHandler = new Mock<ITestHandler>();
            mockTestManager = new Mock<ITestManager>();
            marketingTestingEvents = new FakeMarketingTestingEvents();

            mockServiceLocator = new Mock<IServiceLocator>();
            mockServiceLocator.Setup(sl => sl.GetInstance<ITestHandler>()).Returns(mockTestHandler.Object);
            mockServiceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(mockTestManager.Object);
            mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>())
                .Returns(marketingTestingEvents);

            return new FeatureEnabler(mockServiceLocator.Object);
        }
    }
}
