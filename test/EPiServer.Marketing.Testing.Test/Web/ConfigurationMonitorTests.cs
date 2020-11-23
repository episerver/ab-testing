using EPiServer.Data.Dynamic;
using EPiServer.Framework.Cache;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationMonitorTests
    {
        private Mock<ITestHandler> mockTestHandler;
        private Mock<ITestManager> mockTestManager;
        private Mock<ICacheSignal> mockSignal;
        private Mock<IServiceLocator> mockServiceLocator;
        private Mock<DynamicDataStore> ddsMock;
        private Mock<ISynchronizedObjectInstanceCache> mockSynchronizedObjectInstanceCache;

        public ConfigurationMonitor GetUnitUnderTest(List<IMarketingTest> tests = null)
        {
            // mock the datastore in epi
            ddsMock = new Mock<DynamicDataStore>(null);
            var ddsFactoryMock = new Mock<DynamicDataStoreFactory>();
            ddsFactoryMock.Setup(x => x.GetStore(typeof(AdminConfigTestSettings))).Returns(ddsMock.Object);
            DynamicDataStoreFactory.Instance = ddsFactoryMock.Object;

            AdminConfigTestSettings._factory = ddsFactoryMock.Object;

            ddsMock.Setup(x => x.LoadAll<AdminConfigTestSettings>()).Returns(new List<AdminConfigTestSettings>() {
                new AdminConfigTestSettings() { Id = Data.Identity.NewIdentity() }
            });

            mockTestHandler = new Mock<ITestHandler>();
            mockTestManager = new Mock<ITestManager>();
            var testsReturned = tests == null ? new List<IMarketingTest> { } : tests;
            mockTestManager.Setup(t => t.GetTestList(It.IsAny<TestCriteria>())).Returns(testsReturned);

            mockSignal = new Mock<ICacheSignal>();
            mockTestHandler.Setup(t => t.EnableABTesting()).Verifiable();
            mockTestHandler.Setup(t => t.DisableABTesting()).Verifiable();

            mockSynchronizedObjectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>();

            mockServiceLocator = new Mock<IServiceLocator>();
            mockServiceLocator.Setup(sl => sl.GetInstance<ITestHandler>()).Returns(mockTestHandler.Object);
            mockServiceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(mockTestManager.Object);
            mockServiceLocator.Setup(sl => sl.GetInstance<ISynchronizedObjectInstanceCache>()).Returns(mockSynchronizedObjectInstanceCache.Object);

            return new ConfigurationMonitor(mockServiceLocator.Object, mockSignal.Object);
        }

        [Fact]
        public void HandleConfigurationChange_DoesNotEnableAB_When_EnabledInConfig_And_There_Are_No_ActiveTests()
        {
            var configMonitor = GetUnitUnderTest();

            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Once); 
            mockSignal.Verify(s => s.Reset(), Times.Never);
            mockSignal.Verify(s => s.Set(), Times.Once);
        }

        [Fact]
        public void HandleConfigurationChange_DisablesAB_When_DisabledInConfig()
        {
            var configMonitor = GetUnitUnderTest();

            var expectedConfig = new AdminConfigTestSettings()
            {
                Id = Data.Identity.NewIdentity(),
                AutoPublishWinner = false,
                ConfidenceLevel = 50,
                IsEnabled = false,
                CookieDelimeter = ":",
                KpiLimit = 25,
                ParticipationPercent = 42,
                TestDuration = 182
            };
            ddsMock.Setup(x => x.LoadAll<AdminConfigTestSettings>()).Returns(new List<AdminConfigTestSettings> { expectedConfig });

            AdminConfigTestSettings.Reset();
            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Once); 
            mockSignal.Verify(s => s.Reset(), Times.Never);
            mockSignal.Verify(s => s.Set(), Times.Once);
        }

        [Fact]
        public void ConfigurationMonitor_Ctor_AddsMonitor()
        {
            GetUnitUnderTest();
            mockSignal.Verify(s => s.Monitor(It.Is<Action>(a => a.Method.Name == "HandleConfigurationChange")), Times.Once());
        }
    }
}

