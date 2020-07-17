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

        public ConfigurationMonitor GetUnitUnderTest(List<IMarketingTest> tests = null)
        {
            mockTestHandler = new Mock<ITestHandler>();
            mockTestManager = new Mock<ITestManager>();
            var testsReturned = tests == null ? new List<IMarketingTest> { } : tests;
            mockTestManager.Setup(t => t.GetActiveTests()).Returns(testsReturned);

            mockSignal = new Mock<ICacheSignal>();
            mockTestHandler.Setup(t => t.EnableABTesting()).Verifiable();
            mockTestHandler.Setup(t => t.DisableABTesting()).Verifiable();

            mockServiceLocator = new Mock<IServiceLocator>();
            mockServiceLocator.Setup(sl => sl.GetInstance<ITestHandler>()).Returns(mockTestHandler.Object);
            mockServiceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(mockTestManager.Object);

            return new ConfigurationMonitor(mockServiceLocator.Object, mockSignal.Object);
        }

        [Fact]
        public void HandleConfigurationChange_EnablesAB_When_EnabledInConfig_And_There_Is_Atleast_One_ActiveTest()
        {
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = true };
            var tests = new List<IMarketingTest> { new ABTest { State = TestState.Active }, new ABTest { State = TestState.Archived } };

            var configMonitor = GetUnitUnderTest(tests);
            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Exactly(2));
            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Never);
            mockSignal.Verify(s => s.Reset(), Times.Never);
            mockSignal.Verify(s => s.Set(), Times.Exactly(2));
        }

        [Fact]
        public void HandleConfigurationChange_DoesNotEnableAB_When_EnabledInConfig_And_There_Are_No_ActiveTests()
        {
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = true };
            var configMonitor = GetUnitUnderTest();

            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Exactly(2)); // once in constructor, once in HandleConfigurationChange
            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Never);
            mockSignal.Verify(s => s.Reset(), Times.Never);
            mockSignal.Verify(s => s.Set(), Times.Exactly(2));
        }

        [Fact]
        public void HandleConfigurationChange_DisablesAB_When_DisabledInConfig()
        {
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = false };

            var configMonitor = GetUnitUnderTest();

            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Exactly(2)); 
            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Never);
            mockSignal.Verify(s => s.Reset(), Times.Never);
            mockSignal.Verify(s => s.Set(), Times.Exactly(2));
        }

        [Fact]
        public void HandleResetConfig_Calls_CacheSignal_Reset()
        {
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = false };

            var configMonitor = GetUnitUnderTest();

            configMonitor.Reset();

            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Once); 
            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Never);
            mockSignal.Verify(s => s.Reset(), Times.Once);
            mockSignal.Verify(s => s.Set(), Times.Once);
        }

        [Fact]
        public void ConfigurationMonitor_Ctor_AddsMonitor()
        {
            GetUnitUnderTest();
            mockSignal.Verify(s => s.Set(), Times.Once());
            mockSignal.Verify(s => s.Monitor(It.Is<Action>(a => a.Method.Name == "HandleConfigurationChange")), Times.Once());
        }
    }
}

