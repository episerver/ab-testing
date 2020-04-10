using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.ServiceLocation;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationMonitorTests
    {
        private Mock<ITestHandler> mockTestHandler;
        private Mock<IServiceLocator> mockServiceLocator;

        public ConfigurationMonitor GetUnitUnderTest()
        {
            mockTestHandler = new Mock<ITestHandler>();
            mockTestHandler.Setup(t => t.EnableABTesting()).Verifiable();
            mockTestHandler.Setup(t => t.DisableABTesting()).Verifiable();

            mockServiceLocator = new Mock<IServiceLocator>();
            mockServiceLocator.Setup(sl => sl.GetInstance<ITestHandler>()).Returns(mockTestHandler.Object);

            return new ConfigurationMonitor(mockServiceLocator.Object);
        }

        [Fact]
        public void HandleConfigurationChange_EnablesAB_When_EnabledInConfig()
        {
            var configMonitor = GetUnitUnderTest();

            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = true };

            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Once);
            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Never);
        }

        [Fact]
        public void HandleConfigurationChange_DisablesAB_When_DisabledInConfig()
        {
            var configMonitor = GetUnitUnderTest();

            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = false };

            configMonitor.HandleConfigurationChange();

            mockTestHandler.Verify(t => t.DisableABTesting(), Times.Once);
            mockTestHandler.Verify(t => t.EnableABTesting(), Times.Never);
        }
    }
}
