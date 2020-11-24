using EPiServer.Data.Dynamic;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    [ExcludeFromCodeCoverage]
    public class ABTestConfigStoreTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<ILogger> _logger = new Mock<ILogger>();
        Mock<DynamicDataStoreFactory> _factory = new Mock<DynamicDataStoreFactory>();
        Mock<DynamicDataStore> _store = new Mock<DynamicDataStore>();
        Mock<AdminConfigTestSettings> _settings = new Mock<AdminConfigTestSettings>();
        Mock<IMarketingTestingWebRepository> _webRepo = new Mock<IMarketingTestingWebRepository>();

        private ABTestConfigStore GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ILogger>()).Returns(_logger.Object);
            _locator.Setup(sl => sl.GetInstance<AdminConfigTestSettings>()).Returns(_settings.Object);
            _locator.Setup(sl => sl.GetInstance<IMarketingTestingWebRepository>()).Returns(_webRepo.Object);

            var testStore = new ABTestConfigStore(_locator.Object);
            return testStore;
        }

        public abstract class MyDataStore : DynamicDataStore
        {
            private static StoreDefinition storeDefiniton;

            public MyDataStore() : base(storeDefiniton)
            {

            }
            public MyDataStore(StoreDefinition storeDefiniton) : base(storeDefiniton)
            {
            }

            override public IOrderedQueryable<TResult> Items<TResult>()
            {
                Mock<IOrderedQueryable<TResult>> q = new Mock<IOrderedQueryable<TResult>>();
                IList<TResult> list = new List<TResult>();
                q.Setup(x => x.Where(It.IsAny<Expression<Func<TResult, bool>>>())).Returns(list.AsQueryable);

                return q.Object;
            }
        }

        [Fact]
        public void AdminConfigTestSettings_SavesSettings()
        {
            // mock the datastore in epi
            var ddsMock = new Mock<DynamicDataStore>(null);
            var ddsFactoryMock = new Mock<DynamicDataStoreFactory>();
            ddsFactoryMock.Setup(x => x.GetStore(typeof(AdminConfigTestSettings))).Returns(ddsMock.Object);
            DynamicDataStoreFactory.Instance = ddsFactoryMock.Object;

            AdminConfigTestSettings._factory = ddsFactoryMock.Object;
            AdminConfigTestSettings._currentSettings = null;

            var settings = new AdminConfigTestSettings();
            settings._serviceLocator = _locator.Object;
            _locator.Setup(sl => sl.GetInstance<IMarketingTestingWebRepository>()).Returns(_webRepo.Object);

            settings.Save();

            ddsFactoryMock.Verify();
            ddsMock.Verify(d => d.Save(It.Is<AdminConfigTestSettings>(s => s == settings)));
            _webRepo.Verify(w => w.ConfigurationChanged());
        }

        [Fact]
        public void GetCurrent_ReturnsExpectedDefaults()
        {
            // mock the datastore in epi
            var ddsMock = new Mock<DynamicDataStore>(null);
            var ddsFactoryMock = new Mock<DynamicDataStoreFactory>();
            ddsFactoryMock.Setup(x => x.GetStore(typeof(AdminConfigTestSettings))).Returns(ddsMock.Object);
            DynamicDataStoreFactory.Instance = ddsFactoryMock.Object;

            AdminConfigTestSettings._factory = ddsFactoryMock.Object;
            AdminConfigTestSettings._currentSettings = null;

            // set the mock return value with null cookie delimeter, MAR-1261 - in this case what happens is when an upgrade is performed the value field is never defaulted and 
            // it ends up being null causing all sorts of problems in loadedcontent
            ddsMock.Setup(x => x.LoadAll<AdminConfigTestSettings>()).Returns(new List<AdminConfigTestSettings>() {
                new AdminConfigTestSettings() { Id = Data.Identity.NewIdentity() }
            });

            var testConfig = AdminConfigTestSettings.Current;

            ddsFactoryMock.Verify();
            Assert.Equal(30, testConfig.TestDuration);
            Assert.Equal(10, testConfig.ParticipationPercent);
            Assert.Equal(95, testConfig.ConfidenceLevel);
            Assert.False(testConfig.AutoPublishWinner);
            Assert.Equal(5, testConfig.KpiLimit);
            Assert.NotNull(testConfig.CookieDelimeter);
            Assert.Equal("_", testConfig.CookieDelimeter);
            Assert.True(testConfig.IsEnabled);
        }

        [Fact]
        public void GetCurrent_CallsDDSFactoryGetStore_ToFetchAdminConfigSettings()
        {
            // mock the datastore in epi
            var ddsMock = new Mock<DynamicDataStore>(null);
            var ddsFactoryMock = new Mock<DynamicDataStoreFactory>();
            ddsFactoryMock.Setup(x => x.GetStore(typeof(AdminConfigTestSettings))).Returns(ddsMock.Object);
            DynamicDataStoreFactory.Instance = ddsFactoryMock.Object;

            AdminConfigTestSettings._factory = ddsFactoryMock.Object;
            AdminConfigTestSettings._currentSettings = null;

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

            ddsMock.Setup(x => x.LoadAll<AdminConfigTestSettings>())
                .Returns(new List<AdminConfigTestSettings> { expectedConfig });

            var actualConfig = AdminConfigTestSettings.Current;

            ddsFactoryMock.Verify();
        }

        [Fact]
        public void GetCurrent_ReturnsValid_CookieDeliminator_WhenItsNotInConfig()
        {
            // mock the datastore in epi
            var ddsMock = new Mock<DynamicDataStore>(null);
            var ddsFactoryMock = new Mock<DynamicDataStoreFactory>();
            ddsFactoryMock.Setup(x => x.GetStore(typeof(AdminConfigTestSettings))).Returns(ddsMock.Object);
            DynamicDataStoreFactory.Instance = ddsFactoryMock.Object;

            AdminConfigTestSettings._factory = ddsFactoryMock.Object;
            AdminConfigTestSettings._currentSettings = null;

            // set the mock return value with null cookie delimeter, MAR-1261 - in this case what happens is when an upgrade is performed the value field is never defaulted and 
            // it ends up being null causing all sorts of problems in loadedcontent
            ddsMock.Setup(x => x.LoadAll<AdminConfigTestSettings>()).Returns(new List<AdminConfigTestSettings>() {
                new AdminConfigTestSettings() { Id = Data.Identity.NewIdentity(), CookieDelimeter = null }
            });

            var testConfig = AdminConfigTestSettings.Current;

            ddsFactoryMock.Verify();
            Assert.NotNull(testConfig.CookieDelimeter);
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
            Assert.True((result.Data as AdminConfigTestSettings).AutoPublishWinner == false, "AdminConfigTestSettings.AutoPublishWinner value does not match");
            Assert.True((result.Data as AdminConfigTestSettings).KpiLimit == 5, "AdminConfigTestSettings.KpiLimit value does not match");
            Assert.True((result.Data as AdminConfigTestSettings).CookieDelimeter == "_", "AdminConfigTestSettings.CookieDelimeter value does not match");
            Assert.True((result.Data as AdminConfigTestSettings).IsEnabled, "AdminConfigTestSettings.IsEnabled value does not match");
        }

        [Fact]
        public void Reset_ForcesADatabaseCall()
        {
            // mock the datastore in epi
            var ddsMock = new Mock<DynamicDataStore>(null);
            var ddsFactoryMock = new Mock<DynamicDataStoreFactory>();
            ddsFactoryMock.Setup(x => x.GetStore(typeof(AdminConfigTestSettings))).Returns(ddsMock.Object);
            DynamicDataStoreFactory.Instance = ddsFactoryMock.Object;

            AdminConfigTestSettings._factory = ddsFactoryMock.Object;
 
            ddsMock.Setup(x => x.LoadAll<AdminConfigTestSettings>()).Returns(new List<AdminConfigTestSettings>() {
                new AdminConfigTestSettings() { Id = Data.Identity.NewIdentity() }
            });

            AdminConfigTestSettings.Reset();                     
            var testConfig = AdminConfigTestSettings.Current;   // call the mock once
            testConfig = AdminConfigTestSettings.Current;       // get it twice

            ddsMock.Verify(x => x.LoadAll<AdminConfigTestSettings>(),Times.Once());
 
        }
    }
}
