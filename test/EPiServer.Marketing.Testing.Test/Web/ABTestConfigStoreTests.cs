using System;
using EPiServer.Data.Dynamic;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ABTestConfigStoreTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<ILogger> _logger = new Mock<ILogger>();
        Mock<DynamicDataStoreFactory> _factory = new Mock<DynamicDataStoreFactory>();
        Mock<DynamicDataStore> _store = new Mock<DynamicDataStore>();
        Mock<AdminConfigTestSettings> _settings = new Mock<AdminConfigTestSettings>();

        private ABTestConfigStore GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ILogger>()).Returns(_logger.Object);
            _locator.Setup(sl => sl.GetInstance<AdminConfigTestSettings>()).Returns(_settings.Object);
            var testStore = new ABTestConfigStore(_locator.Object);
            return testStore;
        }

        [Fact]
        public void Get_Returns_Valid_Response()
        {
            var testClass = GetUnitUnderTest();
            var result = testClass.Get() as RestResult;

            Assert.True(result.Data is AdminConfigTestSettings, "Data in result is not AdminConfigTestSettings instance.");
            Assert.True((result.Data as AdminConfigTestSettings).ConfidenceLevel == 95, "AdminConfigTestSettings.ConfidenceLevel value does not match");
            Assert.True((result.Data as AdminConfigTestSettings).ParticipationPercent == 10, "AdminConfigTestSettings.ParticipationPercent value does not match");
            Assert.True((result.Data as AdminConfigTestSettings).TestDuration == 30, "AdminConfigTestSettings.TestDuration value does not match");
        }

        [Fact]
        public void Reset_Current_Settings()
        {
            var id = Guid.NewGuid();
            var settings = new AdminConfigTestSettings() { Id = id };

            Assert.Equal(95, settings.ConfidenceLevel);
            Assert.Equal(30, settings.TestDuration);
            Assert.Equal(10, settings.ParticipationPercent);
            Assert.Equal(id, settings.Id);

            settings.Reset();

            Assert.Null(AdminConfigTestSettings._currentSettings);

        }

    }
}
