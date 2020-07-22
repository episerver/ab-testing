using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.ServiceLocation;
using Moq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    [ExcludeFromCodeCoverage]
    public class FeatureEnablerTests
    {
        private IMarketingTestingEvents marketingTestingEvents;
        private Mock<IServiceLocator> mockServiceLocator;
        private Mock<IConfigurationMonitor> mockConfigurationMonitor;

        private FeatureEnabler GetUnitUnderTest()
        {
            mockConfigurationMonitor = new Mock<IConfigurationMonitor>();
            marketingTestingEvents = new FakeMarketingTestingEvents();

            mockServiceLocator = new Mock<IServiceLocator>();
            mockServiceLocator.Setup(sl => sl.GetInstance<IConfigurationMonitor>()).Returns(mockConfigurationMonitor.Object);
            mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>())
                .Returns(marketingTestingEvents);

            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = true };

            return new FeatureEnabler(mockServiceLocator.Object);
        }

        [Fact]
        public void OnTestAddedToCache_CallsHandleConfigurationChange()
        {
            var featureEnabler = GetUnitUnderTest();
      
            featureEnabler.TestAddedToCache(null, null);
            mockConfigurationMonitor.Verify(t => t.HandleConfigurationChange(), Times.Once);
        }

        [Fact]
        public void OnTestRemovedFromCache_CallsHandleConfigurationChange()
        {
            var featureEnabler = GetUnitUnderTest();

            featureEnabler.TestRemovedFromCache(null, null);
            mockConfigurationMonitor.Verify(t => t.HandleConfigurationChange(), Times.Once);
        }

        [Fact]
        public void Constructor_Adds_EventHandlers()
        {
            GetUnitUnderTest();
            Assert.Equal(1, (marketingTestingEvents as FakeMarketingTestingEvents).TestAddedToCacheCounter);
            Assert.Equal(1, (marketingTestingEvents as FakeMarketingTestingEvents).TestRemovedFromCacheCounter);
        }
    }
}
