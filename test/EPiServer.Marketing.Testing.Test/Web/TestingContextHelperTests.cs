using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;
using System.Globalization;
using EPiServer.Globalization;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Web.Routing;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.KPI.Common.Helpers;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestingContextHelperTests
    {
        private Mock<IServiceLocator> _mockServiceLocator = new Mock<IServiceLocator>();
        private Mock<IContentRepository> _mockContentRepository;
        private Mock<IContentVersionRepository> _mockContentVersionRepository;
        private Mock<IUIHelper> _mockUIHelper;
        private Mock<IKpiManager> _mockKpiManager;
        private Mock<IHttpContextHelper> _mockContextHelper;
        private Mock<IEpiserverHelper> _mockEpiserverHelper;
        private Mock<IKpiHelper> _mockKpiHelper = new Mock<IKpiHelper>();
        private Mock<IContent> testPublishedData = new Mock<IContent>();
        private Mock<IContent> testVariantData = new Mock<IContent>();
        private Mock<IContent> testConversionContent = new Mock<IContent>();

        LocalizationService _localizationService = new FakeLocalizationService("test");

        private IMarketingTest test;
        private Guid testId = Guid.Parse("98cbde0e-4431-4712-9f04-8094e7be826b");
        private CommerceData commerceData;

        private TestingContextHelper GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();
            _mockContentRepository = new Mock<IContentRepository>();
            _mockKpiManager = new Mock<IKpiManager>();
            _mockContentVersionRepository = new Mock<IContentVersionRepository>();
            _mockUIHelper = new Mock<IUIHelper>();
            _mockContextHelper = new Mock<IHttpContextHelper>();
            _mockEpiserverHelper = new Mock<IEpiserverHelper>();
            testPublishedData = new Mock<IContent>();
            testVariantData = new Mock<IContent>();
            testConversionContent = new Mock<IContent>();           

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
            ContentLanguage.PreferredCulture = new CultureInfo("en");

            _mockServiceLocator.Setup(call => call.GetInstance<IContentRepository>())
              .Returns(_mockContentRepository.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IContentVersionRepository>())
                .Returns(_mockContentVersionRepository.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IKpiHelper>()).Returns(_mockKpiHelper.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IUIHelper>()).Returns(_mockUIHelper.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IKpiManager>()).Returns(_mockKpiManager.Object);
            _mockServiceLocator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(_localizationService);


            return new TestingContextHelper(_mockContextHelper.Object, _mockServiceLocator.Object, _mockEpiserverHelper.Object);
        }

        [Fact]
        public void IsInSystemFolder_returns_true_if_context_is_null()
        {
            var testContextHelper = GetUnitUnderTest();
            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(false);

            var swapDisabled = testContextHelper.IsInSystemFolder();

            Assert.True(swapDisabled);
        }

        [Fact]
        public void SwapDisabled_returns_false_when_we_are_able_to_swap()
        {
            var contentArgs = new ContentEventArgs(new ContentReference(1));
            contentArgs.Content = new PageData();

            var testContextHelper = GetUnitUnderTest();
            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasUserAgent()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasItem(It.IsAny<string>())).Returns(false);
            _mockContextHelper.Setup(ch => ch.RequestedUrl()).Returns("myUrl");
            _mockEpiserverHelper.Setup(ch => ch.GetRootPath()).Returns("adifferentUrl");

            var swapDisabled = testContextHelper.SwapDisabled(contentArgs);

            Assert.False(swapDisabled);
        }

        [Fact]
        public void SwapDisabled_returns_false_if_args_are_not_the_correct_type()
        {
            var testContextHelper = GetUnitUnderTest();
            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasUserAgent()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasItem(It.IsAny<string>())).Returns(false);
            _mockContextHelper.Setup(ch => ch.RequestedUrl()).Returns("myUrl");
            _mockEpiserverHelper.Setup(ch => ch.GetRootPath()).Returns("adifferentUrl");

            var swapDisabled = testContextHelper.SwapDisabled(new EventArgs());

            Assert.False(swapDisabled);
        }

        [Fact]
        public void SwapDisabled_returns_true_if_there_is_no_content_to_check()
        {
            var contentArgs = new ContentEventArgs(new ContentReference(1));

            var testContextHelper = GetUnitUnderTest();
            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasUserAgent()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasItem(It.IsAny<string>())).Returns(false);
            _mockContextHelper.Setup(ch => ch.RequestedUrl()).Returns("myUrl");
            _mockEpiserverHelper.Setup(ch => ch.GetRootPath()).Returns("adifferentUrl");

            var swapDisabled = testContextHelper.SwapDisabled(contentArgs);

            Assert.True(swapDisabled);
        }

        [Fact]
        public void SwapDisabled_returns_true_if_we_should_skip_the_request()
        {
            var contentArgs = new ContentEventArgs(new ContentReference(1));
            contentArgs.Content = new PageData();

            var testContextHelper = GetUnitUnderTest();
            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasUserAgent()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasItem(It.IsAny<string>())).Returns(true);
            _mockContextHelper.Setup(ch => ch.RequestedUrl()).Returns("myUrl");
            _mockEpiserverHelper.Setup(ch => ch.GetRootPath()).Returns("adifferentUrl");

            var swapDisabled = testContextHelper.SwapDisabled(contentArgs);

            Assert.True(swapDisabled);
        }

        [Fact]
        public void SwapDisabled_returns_true_if_useragent_is_null()
        {
            var contentArgs = new ContentEventArgs(new ContentReference(1));
            contentArgs.Content = new PageData();

            var testContextHelper = GetUnitUnderTest();

            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasUserAgent()).Returns(false);
            _mockContextHelper.Setup(ch => ch.HasItem(It.IsAny<string>())).Returns(true);
            _mockContextHelper.Setup(ch => ch.RequestedUrl()).Returns("myUrl");
            _mockEpiserverHelper.Setup(ch => ch.GetRootPath()).Returns("adifferentUrl");

            var swapDisabled = testContextHelper.SwapDisabled(contentArgs);

            Assert.True(swapDisabled);
        }

        [Fact]
        public void SwapDisabled_with_ChildrenEventArgs_returns_false_when_we_are_able_to_swap()
        {
            var contentArgs = new ChildrenEventArgs(new ContentReference(1), new List<IContent>());

            var testContextHelper = GetUnitUnderTest();
            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasUserAgent()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasItem(It.IsAny<string>())).Returns(false);
            _mockContextHelper.Setup(ch => ch.RequestedUrl()).Returns("myUrl");
            _mockEpiserverHelper.Setup(ch => ch.GetRootPath()).Returns("adifferentUrl");

            var swapDisabled = testContextHelper.SwapDisabled(contentArgs);

            Assert.False(swapDisabled);
        }

        [Fact]
        public void SwapDisabled_with_ChildrenEventArgs_returns_true_when_there_is_no_content_in_agruments()
        {
            var contentArgs = new ChildrenEventArgs(new ContentReference(1), new List<IContent>());
            contentArgs.ContentLink = null;

            var testContextHelper = GetUnitUnderTest();
            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasUserAgent()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasItem(It.IsAny<string>())).Returns(false);
            _mockContextHelper.Setup(ch => ch.RequestedUrl()).Returns("myUrl");
            _mockEpiserverHelper.Setup(ch => ch.GetRootPath()).Returns("adifferentUrl");

            var swapDisabled = testContextHelper.SwapDisabled(contentArgs);

            Assert.True(swapDisabled);
        }

        [Fact]
        public void SwapDisabled_with_ChildrenEventArgs_returns_true_when_there_is_no_children_in_argument()
        {
            var contentArgs = new ChildrenEventArgs(new ContentReference(1), new List<IContent>());
            contentArgs.ChildrenItems = null;

            var testContextHelper = GetUnitUnderTest();
            _mockContextHelper.Setup(ch => ch.HasCurrentContext()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasUserAgent()).Returns(true);
            _mockContextHelper.Setup(ch => ch.HasItem(It.IsAny<string>())).Returns(false);
            _mockContextHelper.Setup(ch => ch.RequestedUrl()).Returns("myUrl");
            _mockEpiserverHelper.Setup(ch => ch.GetRootPath()).Returns("adifferentUrl");

            var swapDisabled = testContextHelper.SwapDisabled(contentArgs);

            Assert.True(swapDisabled);
        }

        [Fact]
        public void GenerateContextData_returns_valid_contextdatamodel()
        {
            commerceData = new CommerceData()
            {
                preferredFormat = new NumberFormatInfo(),
                CommerceCulture = "USD"
            };

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
                IsPublished = true,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
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
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
            };

            var kpi = new ContentComparatorKPI(_mockServiceLocator.Object, Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            var testContextHelper = GetUnitUnderTest();
            testVariantData.Setup(call => call.ContentLink).Returns(new ContentReference(5, 197));
            testConversionContent.Setup(call => call.ContentLink).Returns(new ContentReference(8, 89));
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(testVariantData.Object);
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(testConversionContent.Object);
            _mockContentVersionRepository.Setup(
               call => call.Load(It.IsAny<ContentReference>()))
               .Returns(new ContentVersion(new ContentReference(10, 100), "testName", VersionStatus.CheckedOut, DateTime.Now, "me", "me", 0, "en", false, false));

            MarketingTestingContextModel testResult = testContextHelper.GenerateContextData(test);

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
            commerceData = new CommerceData()
            {
                preferredFormat = new NumberFormatInfo(),
                CommerceCulture = "USD"
            };

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
                IsPublished = true,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
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
                IsPublished = false,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
            };

            var kpi = new ContentComparatorKPI(_mockServiceLocator.Object, Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                //PreferredCulture = "en-GB"
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            var testContextHelper = GetUnitUnderTest();
            testVariantData.Setup(call => call.ContentLink).Returns(new ContentReference(5, 197));
            testConversionContent.Setup(call => call.ContentLink).Returns(new ContentReference(8, 89));
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(testVariantData.Object);
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(testConversionContent.Object);
            _mockContentVersionRepository.Setup(
               call => call.Load(It.IsAny<ContentReference>()))
               .Returns(new ContentVersion(new ContentReference(10, 100), "testName", VersionStatus.CheckedOut, DateTime.Now, "me", "me", 0, "en", false, false));
           
            MarketingTestingContextModel testResult = testContextHelper.GenerateContextData(test);

            Assert.NotNull(testResult);
            Assert.True(testResult.Test.Id == testId);
            Assert.True(testResult.TotalParticipantCount == (draftVariant.Views + publishedVariant.Views));
            Assert.True(testResult.PublishedVersionPublishedBy == "me");
            Assert.True(testResult.DaysRemaining == "0");
            Assert.True(testResult.DaysElapsed == "5");
            Assert.True(testResult.DraftVersionChangedBy == "me");
            Assert.NotNull(testResult.DraftVersionChangedDate);
            Assert.True(testResult.DraftVersionChangedBy == "me");
            Assert.True(testResult.KpiResultType == "KpiConversionResult");
            Assert.NotNull(testResult.LatestVersionContentLink);
            Assert.NotNull(testResult.PublishedVersionPublishedDate);
            Assert.True(testResult.UserHasPublishRights);
            Assert.Equal(testResult.VisitorPercentage, "30");
        }

        [Fact]
        public void Test_Marked_Archived_Returns_Correct_Elapsed_and_Remaining_values()
        {

            commerceData = new CommerceData()
            {
                preferredFormat = new NumberFormatInfo(),
                CommerceCulture = "USD"
            };

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
                IsPublished = true,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
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
                IsPublished = false,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
            };

            var kpi = new ContentComparatorKPI(_mockServiceLocator.Object, Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                //PreferredCulture = "en-GB"
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            var testContextHelper = GetUnitUnderTest();
            testVariantData.Setup(call => call.ContentLink).Returns(new ContentReference(5, 197));
            testConversionContent.Setup(call => call.ContentLink).Returns(new ContentReference(8, 89));
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(testVariantData.Object);
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(testConversionContent.Object);
            _mockContentVersionRepository.Setup(
               call => call.Load(It.IsAny<ContentReference>()))
               .Returns(new ContentVersion(new ContentReference(10, 100), "testName", VersionStatus.CheckedOut, DateTime.Now, "me", "me", 0, "en", false, false));
            MarketingTestingContextModel testResult = testContextHelper.GenerateContextData(test);

            Assert.NotNull(testResult);
            Assert.True(testResult.Test.Id == testId);
            Assert.True(testResult.TotalParticipantCount == (draftVariant.Views + publishedVariant.Views));
            Assert.True(testResult.PublishedVersionPublishedBy == "me");
            Assert.True(testResult.DaysRemaining == "0");
            Assert.True(testResult.DaysElapsed == "9");
        }

        [Fact]
        public void GenerateContextData_processes_the_expected_days_remaining_for_active_tests()
        {

            commerceData = new CommerceData()
            {
                preferredFormat = new NumberFormatInfo(),
                CommerceCulture = "USD"
            };

            test = new ABTest()
            {
                CreatedDate = DateTime.Parse("5/5/2016"),
                Description = "Unit Test Description",
                StartDate = DateTime.Now.AddDays(-2),
                EndDate = DateTime.Now.AddDays(10),
                Id = testId,
                LastModifiedBy = "Unit Test User",
                ModifiedDate = DateTime.Parse("5/4/2016"),
                OriginalItemId = Guid.Parse("92de6b63-1dce-4669-bfa5-725e9aea1664"),
                Owner = "Unit Test",
                ParticipationPercentage = 30,
                State = TestState.Active
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
                IsPublished = true,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
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
                IsPublished = false,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
            };

            var testContextHelper = GetUnitUnderTest();

            testVariantData.Setup(call => call.ContentLink).Returns(new ContentReference(5, 197));
            testConversionContent.Setup(call => call.ContentLink).Returns(new ContentReference(8, 89));
             _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(testVariantData.Object);
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(testConversionContent.Object);
            _mockContentVersionRepository.Setup(
               call => call.Load(It.IsAny<ContentReference>()))
               .Returns(new ContentVersion(new ContentReference(10, 100), "testName", VersionStatus.CheckedOut, DateTime.Now, "me", "me", 0, "en", false, false));
            _mockUIHelper.Setup(call => call.getEpiUrlFromLink(It.IsAny<ContentReference>())).Returns("TestLink");
           
            var kpi = new ContentComparatorKPI(_mockServiceLocator.Object, Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            MarketingTestingContextModel testResult = testContextHelper.GenerateContextData(test);

            Assert.True(testResult.DaysElapsed == 2.ToString());
            Assert.True(testResult.DaysRemaining == 10.ToString());
        }

        [Fact]
        public void Model_Contains_Published_And_Draft_Preview_Urls()
        {
            commerceData = new CommerceData()
            {
                preferredFormat = new NumberFormatInfo(),
                CommerceCulture = "USD"
            };

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
                IsPublished = true,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
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
                IsPublished = false,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
            };

            var kpi = new ContentComparatorKPI(_mockServiceLocator.Object, Guid.Parse("10acbb11-693a-4f20-8602-b766152bf3bb"))
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                //PreferredCulture = "en-GB"
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };
            test.KpiInstances = new List<IKpi>() { kpi };

            var testContextHelper = GetUnitUnderTest();
            testVariantData.Setup(call => call.ContentLink).Returns(new ContentReference(5, 197));
            testConversionContent.Setup(call => call.ContentLink).Returns(new ContentReference(8, 89));
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(testVariantData.Object);
            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(testConversionContent.Object);
            _mockContentVersionRepository.Setup(
               call => call.Load(It.IsAny<ContentReference>()))
               .Returns(new ContentVersion(new ContentReference(10, 100), "testName", VersionStatus.CheckedOut, DateTime.Now, "me", "me", 0, "en", false, false));
            _mockEpiserverHelper.Setup(pub => pub.GetPreviewUrl(It.IsAny<ContentReference>(), It.IsAny<string>(), It.IsAny<VirtualPathArguments>())).Returns("previewUrl");

            var result = testContextHelper.GenerateContextData(test);
            Assert.False(string.IsNullOrEmpty(result.PublishPreviewUrl));
            Assert.False(string.IsNullOrEmpty(result.DraftPreviewUrl));
        }

        [Fact]
        public void IsRequestedContent_is_true_when_the_loaded_page_matches_the_one_in_the_request()
        {
            var aPage = new PageData(new PageReference(1));

            var testContextHelper = GetUnitUnderTest();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IPageRouteHelper>()).Returns(new FakePageRouteHelper(aPage));

            var result = testContextHelper.IsRequestedContent(aPage);

            Assert.True(result);
        }

        [Fact]
        public void IsRequestedContent_is_false_when_the_loaded_page_is_not_the_requested_page()
        {
            var aPage = new PageData(new PageReference(1));
            var aPage2 = new PageData(new PageReference(2));

            var testContextHelper = GetUnitUnderTest();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IPageRouteHelper>()).Returns(new FakePageRouteHelper(aPage));

            var result = testContextHelper.IsRequestedContent(aPage2);

            Assert.False(result);
        }

        [Fact]
        public void IsRequestedContent_is_true_when_the_loaded_content_is_not_a_page()
        {
            var aBlock = new MediaData();

            var testContextHelper = GetUnitUnderTest();

            var result = testContextHelper.IsRequestedContent(aBlock);

            Assert.True(result);
        }

        [Fact]
        public void IsRequestedContent_is_false_when_the_we_cant_determine_the_requested_page()
        {
            var aPage = new PageData(new PageReference(1));

            var testContextHelper = GetUnitUnderTest();

            var result = testContextHelper.IsRequestedContent(aPage);

            Assert.False(result);
        }

        [Fact]
        public void SwapDisabled_with_childevent_args_is_true_when_we_cant_figure_out_what_content_was_loaded()
        {
            var testContextHelper = GetUnitUnderTest();

        }
    }
}
