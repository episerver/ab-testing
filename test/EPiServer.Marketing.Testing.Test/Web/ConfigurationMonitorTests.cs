using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationMonitorTests
    {
        private Mock<ITestHandler> mockTestHandler;
        private Mock<ICacheSignal> mockSignal;
        private Mock<IServiceLocator> mockServiceLocator;

        public ConfigurationMonitor GetUnitUnderTest()
        {
            mockTestHandler = new Mock<ITestHandler>();
            mockSignal = new Mock<ICacheSignal>();
            mockTestHandler.Setup(t => t.EnableABTesting()).Verifiable();
            mockTestHandler.Setup(t => t.DisableABTesting()).Verifiable();

            mockServiceLocator = new Mock<IServiceLocator>();
            mockServiceLocator.Setup(sl => sl.GetInstance<ITestHandler>()).Returns(mockTestHandler.Object);

            return new ConfigurationMonitor(mockServiceLocator.Object, mockSignal.Object);
        }

        [Fact]
        public void HandleConfigurationChange_EnablesAB_When_EnabledInConfig()
        {
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = true };

            var configMonitor = GetUnitUnderTest();

            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Exactly(2));
            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Never);
        }

        [Fact]
        public void HandleConfigurationChange_DisablesAB_When_DisabledInConfig()
        {
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = false };

            var configMonitor = GetUnitUnderTest();

            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Exactly(2));
            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Never);
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

