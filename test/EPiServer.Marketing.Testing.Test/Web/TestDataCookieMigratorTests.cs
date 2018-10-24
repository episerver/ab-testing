using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestDataCookieMigratorTests
    {
        private Mock<IMarketingTestingWebRepository> _testRepo;
        private Mock<IHttpContextHelper> _httpContextHelper;
        private Mock<IEpiserverHelper> _epiHelper;
        private Mock<IAdminConfigTestSettingsHelper> _settingsHelper;
        private Mock<ITestDataCookieMigrator> _testDataCookieMigrator;

        private Guid _activeTestId = Guid.Parse("a194bde9-af3c-40fa-9635-338d02f5dea4");
        private string _cookieDelimeter = "_";

        private TestDataCookieHelper GetUnitUnderTest()
        {
            _testRepo = new Mock<IMarketingTestingWebRepository>();
            _settingsHelper = new Mock<IAdminConfigTestSettingsHelper>();
            _settingsHelper.Setup(call => call.GetCookieDelimeter()).Returns(_cookieDelimeter);
            _httpContextHelper = new Mock<IHttpContextHelper>();
            _epiHelper = new Mock<IEpiserverHelper>();
            _testDataCookieMigrator = new Mock<ITestDataCookieMigrator>();

            return new TestDataCookieHelper(_settingsHelper.Object, _testRepo.Object, _httpContextHelper.Object, _epiHelper.Object, _testDataCookieMigrator.Object);
        }

        [Fact]
        public void UpdateOldCookie_Correctly_Updates_Cookie()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var variant1 = new Variant()
            {
                Id = Guid.Parse("dee37c30-973b-4e48-9b59-148a6a730ed9"),
                IsPublished = true
            };

            var variant2 = new Variant()
            {
                Id = Guid.Parse("a19221d7-b977-4f90-b256-2e6b3cfd8216"),
            };

            var test = new ABTest()
            {
                Id = _activeTestId,
                StartDate = startDate,
                OriginalItemId = testContentId,
                Variants = new List<Variant>() { variant1, variant2 },
                KpiInstances = new List<IKpi>(),
                ContentLanguage = "en-US"

            };

            var updatedCookie = new TestDataCookie();
            _httpContextHelper.Setup(hch => hch.AddCookie(It.IsAny<HttpCookie>()));
            _testRepo.Setup(call => call.GetTestById(It.IsAny<Guid>(), true)).Returns(test);

            _testDataCookieMigrator.Setup(
                call =>
                    call.UpdateOldCookie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CultureInfo>(),
                        It.IsAny<string>())).Returns(updatedCookie);

            mockTestDataCookiehelper.UpdateOldCookie(testContentId.ToString(), CultureInfo.CurrentCulture, CultureInfo.CurrentCulture.Name);

            _httpContextHelper.Verify(call => call.AddCookie(It.IsAny<HttpCookie>()), Times.Exactly(2));
            _httpContextHelper.Verify(call => call.RemoveCookie(It.Is<string>(s => s == "EPI-MAR-" + testContentId + ":" + CultureInfo.CurrentCulture.Name)), Times.Once);
        }
    }
}
