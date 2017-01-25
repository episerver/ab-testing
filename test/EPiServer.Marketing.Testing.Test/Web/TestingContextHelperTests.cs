using System;
using System.Collections.Generic;
using System.Web;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;
using System.Globalization;
using EPiServer.Globalization;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Web.Routing;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestingContextHelperTests : IDisposable
    {
        private TestingContextHelper _testingContextHelper;
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<IContentRepository> _mockContentRepository;
        private Mock<IContentVersionRepository> _mockContentVersionRepository;
        private Mock<IUIHelper> _mockUIHelper;
        private Mock<IPreviewUrlBuilder> _mockPreviewUrlBuilder;


        LocalizationService _localizationService = new FakeLocalizationService("test");

        private IMarketingTest test;
        private Guid testId = Guid.Parse("98cbde0e-4431-4712-9f04-8094e7be826b");

        private TestingContextHelper GetUnitUnderTest(HttpContext context)
        {
            Mock<IContent> testPublishedData = new Mock<IContent>();
            testPublishedData.Setup(call => call.ContentLink).Returns(new ContentReference(1, 5));
            Mock<IContent> testVariantData = new Mock<IContent>();
            testVariantData.Setup(call => call.ContentLink).Returns(new ContentReference(5, 197));
            Mock<IContent> testConversionContent = new Mock<IContent>();
            testConversionContent.Setup(call => call.ContentLink).Returns(new ContentReference(8, 89));
            testConversionContent.Setup(call => call.Name).Returns("Conversion Content");

            _mockServiceLocator = new Mock<IServiceLocator>();
            _mockContentRepository = new Mock<IContentRepository>();
            _mockContentRepository.Setup(call => call.Get<IContent>(It.Is<Guid>(g => g == Guid.Parse("92de6b63-1dce-4669-bfa5-725e9aea1664")))).Returns(testPublishedData.Object);
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(testPublishedData.Object);
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(testVariantData.Object);
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(testConversionContent.Object);

            _mockContentVersionRepository = new Mock<IContentVersionRepository>();
            _mockContentVersionRepository.Setup(
               call => call.Load(It.IsAny<ContentReference>()))
               .Returns(new ContentVersion(new ContentReference(10, 100), String.Empty, VersionStatus.CheckedOut, DateTime.Now, "me", "me", 0, "en", false, false));

            _mockUIHelper = new Mock<IUIHelper>();
            _mockUIHelper.Setup(call => call.getEpiUrlFromLink(It.IsAny<ContentReference>())).Returns("TestLink");

            _mockServiceLocator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(_localizationService);
            _mockServiceLocator.Setup(call => call.GetInstance<IContentRepository>())
               .Returns(_mockContentRepository.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IContentVersionRepository>())
                .Returns(_mockContentVersionRepository.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IUIHelper>()).Returns(_mockUIHelper.Object);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
            ContentLanguage.PreferredCulture = new CultureInfo("en");

            _mockPreviewUrlBuilder = new Mock<IPreviewUrlBuilder>();

            return new TestingContextHelper(context, _mockServiceLocator.Object, _mockPreviewUrlBuilder.Object);
        }

        [Fact]
        public void IsInSystemFolder_returns_true_if_context_is_null()
        {
            _testingContextHelper = GetUnitUnderTest(null);
            var swapDisabled = _testingContextHelper.IsInSystemFolder();

            Assert.True(swapDisabled);
        }

        [Fact]
        public void GenerateContextData_returns_valid_contextdatamodel()
        {
            test = new ABTest()
            {
                CreatedDate = DateTime.Parse("5/5/2016"),
                Description = "Unit Test Description",
                EndDate = DateTime.Parse("6/6/2016"),
                Id = testId,
                LastModifiedBy = "Unit Test User",
                ModifiedDate = DateTime.Parse("5/4/2016"),
                OriginalItemId = Guid.Parse("92de6b63-1dce-4669-bfa5-725e9aea1664"),
                Owner = "Unit Test",
                ParticipationPercentage = 30
            };

            Variant publishedVariant = new Variant()
            {
                Conversions = 5,
                Id = Guid.Parse("b7a2a5ea-5925-4fe6-b546-a31876dddafb"),
                IsWinner = false,
                ItemId = Guid.Parse("60820307-65d3-459d-8a59-6502c9655735"),
                ItemVersion = 5,
                Views = 25,
                TestId = test.Id,
                IsPublished = true
            };

            Variant draftVariant = new Variant()
            {
                Conversions = 15,
                Id = Guid.Parse("68d8cc5e-39dc-44b7-a784-40c14221c0c1"),
                IsWinner = false,
                ItemId = Guid.Parse("46c3deca-e080-49ae-bbea-51c73af34f34"),
                ItemVersion = 190,
                Views = 50,
                TestId = test.Id
            };

            var kpi = new ContentComparatorKPI(Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                PreferredCommerceFormat = new KPI.Manager.CommerceData { PreferredMarketValue = "nor", preferredFormat = new NumberFormatInfo()}
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            _testingContextHelper = GetUnitUnderTest(null);
            MarketingTestingContextModel testResult = _testingContextHelper.GenerateContextData(test);

            Assert.NotNull(testResult);
            Assert.True(testResult.Test.Id == testId);
            Assert.True(testResult.TotalParticipantCount == (draftVariant.Views + publishedVariant.Views));
            Assert.True(testResult.PublishedVersionContentLink == "10_100");
            Assert.True(testResult.DraftVersionContentLink == "5_197");
            Assert.True(testResult.PublishedVersionPublishedBy == "me");
        }

        [Fact]
        public void Test_Marked_Done_Returns_Correct_Elapsed_and_Remaining_values()
        {
            test = new ABTest()
            {
                CreatedDate = DateTime.Parse("5/5/2016"),
                Description = "Unit Test Description",
                StartDate = DateTime.Parse("6/1/2016"),
                EndDate = DateTime.Parse("6/6/2016"),
                Id = testId,
                LastModifiedBy = "Unit Test User",
                ModifiedDate = DateTime.Parse("5/4/2016"),
                OriginalItemId = Guid.Parse("92de6b63-1dce-4669-bfa5-725e9aea1664"),
                Owner = "Unit Test",
                ParticipationPercentage = 30,
                State = TestState.Done
            };

            Variant publishedVariant = new Variant()
            {
                Conversions = 5,
                Id = Guid.Parse("b7a2a5ea-5925-4fe6-b546-a31876dddafb"),
                IsWinner = false,
                ItemId = Guid.Parse("60820307-65d3-459d-8a59-6502c9655735"),
                ItemVersion = 5,
                Views = 25,
                TestId = test.Id,
                IsPublished = true
            };

            Variant draftVariant = new Variant()
            {
                Conversions = 15,
                Id = Guid.Parse("68d8cc5e-39dc-44b7-a784-40c14221c0c1"),
                IsWinner = false,
                ItemId = Guid.Parse("46c3deca-e080-49ae-bbea-51c73af34f34"),
                ItemVersion = 190,
                Views = 50,
                TestId = test.Id,
                IsPublished = false
            };

            var kpi = new ContentComparatorKPI(Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                //PreferredCulture = "en-GB"
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            _testingContextHelper = GetUnitUnderTest(null);
            MarketingTestingContextModel testResult = _testingContextHelper.GenerateContextData(test);

            Assert.NotNull(testResult);
            Assert.True(testResult.Test.Id == testId);
            Assert.True(testResult.TotalParticipantCount == (draftVariant.Views + publishedVariant.Views));
            Assert.True(testResult.PublishedVersionPublishedBy == "me");
            Assert.True(testResult.DaysRemaining == "0");
            Assert.True(testResult.DaysElapsed == "5");
        }

        [Fact]
        public void Test_Marked_Archived_Returns_Correct_Elapsed_and_Remaining_values()
        {
            test = new ABTest()
            {
                CreatedDate = DateTime.Parse("5/5/2016"),
                Description = "Unit Test Description",
                StartDate = DateTime.Parse("6/1/2016"),
                EndDate = DateTime.Parse("6/10/2016"),
                Id = testId,
                LastModifiedBy = "Unit Test User",
                ModifiedDate = DateTime.Parse("5/4/2016"),
                OriginalItemId = Guid.Parse("92de6b63-1dce-4669-bfa5-725e9aea1664"),
                Owner = "Unit Test",
                ParticipationPercentage = 30,
                State = TestState.Archived
            };

            Variant publishedVariant = new Variant()
            {
                Conversions = 5,
                Id = Guid.Parse("b7a2a5ea-5925-4fe6-b546-a31876dddafb"),
                IsWinner = false,
                ItemId = Guid.Parse("60820307-65d3-459d-8a59-6502c9655735"),
                ItemVersion = 5,
                Views = 25,
                TestId = test.Id,
                IsPublished = true
            };

            Variant draftVariant = new Variant()
            {
                Conversions = 15,
                Id = Guid.Parse("68d8cc5e-39dc-44b7-a784-40c14221c0c1"),
                IsWinner = false,
                ItemId = Guid.Parse("46c3deca-e080-49ae-bbea-51c73af34f34"),
                ItemVersion = 190,
                Views = 50,
                TestId = test.Id,
                IsPublished = false
            };

            var kpi = new ContentComparatorKPI(Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                //PreferredCulture = "en-GB"
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            _testingContextHelper = GetUnitUnderTest(null);
            MarketingTestingContextModel testResult = _testingContextHelper.GenerateContextData(test);

            Assert.NotNull(testResult);
            Assert.True(testResult.Test.Id == testId);
            Assert.True(testResult.TotalParticipantCount == (draftVariant.Views + publishedVariant.Views));
            Assert.True(testResult.PublishedVersionPublishedBy == "me");
            Assert.True(testResult.DaysRemaining == "0");
            Assert.True(testResult.DaysElapsed == "9");
        }

        [Fact]
        public void Model_Contains_Published_And_Draft_Preview_Urls()
        {
            test = new ABTest()
            {
                CreatedDate = DateTime.Parse("5/5/2016"),
                Description = "Unit Test Description",
                EndDate = DateTime.Parse("6/6/2016"),
                Id = testId,
                LastModifiedBy = "Unit Test User",
                ModifiedDate = DateTime.Parse("5/4/2016"),
                OriginalItemId = Guid.Parse("92de6b63-1dce-4669-bfa5-725e9aea1664"),
                Owner = "Unit Test",
                ParticipationPercentage = 30
            };

            Variant publishedVariant = new Variant()
            {
                Conversions = 5,
                Id = Guid.Parse("b7a2a5ea-5925-4fe6-b546-a31876dddafb"),
                IsWinner = false,
                ItemId = Guid.Parse("60820307-65d3-459d-8a59-6502c9655735"),
                ItemVersion = 5,
                Views = 25,
                TestId = test.Id,
                IsPublished = true
            };

            Variant draftVariant = new Variant()
            {
                Conversions = 15,
                Id = Guid.Parse("68d8cc5e-39dc-44b7-a784-40c14221c0c1"),
                IsWinner = false,
                ItemId = Guid.Parse("46c3deca-e080-49ae-bbea-51c73af34f34"),
                ItemVersion = 190,
                Views = 50,
                TestId = test.Id,
                IsPublished = false
            };

            var kpi = new ContentComparatorKPI(Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                //PreferredCulture = "en-GB"
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            _testingContextHelper = GetUnitUnderTest(null);
            _mockPreviewUrlBuilder.Setup(pub => pub.GetPreviewUrl(It.IsAny<ContentReference>(), It.IsAny<string>(), It.IsAny<VirtualPathArguments>())).Returns("previewUrl");

            var result = _testingContextHelper.GenerateContextData(test);
            Assert.False(string.IsNullOrEmpty(result.PublishPreviewUrl));
            Assert.False(string.IsNullOrEmpty(result.DraftPreviewUrl));
        }

        public void Dispose()
        {
            HttpContext.Current = null;
        }
    }
}
