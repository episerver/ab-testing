using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using Xunit;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Messaging;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class WebRepositoryTests
    {
        private Mock<ITestManager> _mockTestManager;
        private Mock<ILogger> _mockLogger;
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<ITestResultHelper> _mockTestResultHelper;
        private Mock<IMarketingTestingWebRepository> _mockMarketingTestingWebRepository;
        private Mock<IKpiManager> _mockKpiManager;
        private Mock<IHttpContextHelper> _mockHttpHelper;
        private Mock<IEpiserverHelper> _mockEpiserverHelper;

        private Guid _testGuid = Guid.Parse("984ae93a-3abc-469f-8664-250328ce8220");

        private MarketingTestingWebRepository GetUnitUnderTest()
        {
            _mockLogger = new Mock<ILogger>();
            _mockServiceLocator = new Mock<IServiceLocator>();

            _mockTestManager = new Mock<ITestManager>();
            _mockTestManager.Setup(call => call.Save(It.IsAny<IMarketingTest>())).Returns(new Guid());
            _mockServiceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_mockTestManager.Object);

            var kpi = new ContentComparatorKPI() { ContentGuid = Guid.NewGuid() };

            _mockKpiManager = new Mock<IKpiManager>();
            _mockKpiManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(kpi);
            _mockServiceLocator.Setup(sl => sl.GetInstance<IKpiManager>()).Returns(_mockKpiManager.Object);


            _mockTestResultHelper = new Mock<ITestResultHelper>();
            _mockServiceLocator.Setup(call => call.GetInstance<ITestResultHelper>())
             .Returns(_mockTestResultHelper.Object);

            _mockMarketingTestingWebRepository = new Mock<IMarketingTestingWebRepository>();
            _mockServiceLocator.Setup(call => call.GetInstance<IMarketingTestingWebRepository>())
                .Returns(_mockMarketingTestingWebRepository.Object);

            _mockHttpHelper = new Mock<IHttpContextHelper>();
            _mockServiceLocator.Setup(call => call.GetInstance<IHttpContextHelper>())
                .Returns(_mockHttpHelper.Object);

            _mockEpiserverHelper = new Mock<IEpiserverHelper>();
            _mockServiceLocator.Setup(call => call.GetInstance<IEpiserverHelper>())
                .Returns(_mockEpiserverHelper.Object);

            var aRepo = new MarketingTestingWebRepository(_mockServiceLocator.Object, _mockLogger.Object);
            return aRepo;
        }

        [Fact]
        public void GetVariantContent_Calls_TestManager_GetVariantContent()
        {
            var aRepo = GetUnitUnderTest();
            aRepo.GetVariantContent(_testGuid);
            _mockTestManager.Verify(called => called.GetVariantContent(It.Is<Guid>(value => value == _testGuid)), Times.Once);
        }

        [Fact]
        public void ReturnLandingPage_Calls_TestManager_ReturnLandingPage()
        {
            var aRepo = GetUnitUnderTest();
            aRepo.ReturnLandingPage(_testGuid);
            _mockTestManager.Verify(called => called.ReturnLandingPage(It.Is<Guid>(value => value == _testGuid)), Times.Once);
        }

        [Fact]
        public void GetActiveTestsByOriginalItemId_Calls_TestManager_GetActiveTestsByOriginalItemId()
        {
            var aRepo = GetUnitUnderTest();
            aRepo.GetActiveTestsByOriginalItemId(_testGuid);
            _mockTestManager.Verify(called => called.GetActiveTestsByOriginalItemId(It.Is<Guid>(value => value == _testGuid)), Times.Once);
        }

        [Fact]
        public void AsynchronousIncrementCount_Calls_TestManager_IncrementCount_WithDefault_AsynchFlag()
        {
            var aRepo = GetUnitUnderTest();
            aRepo.IncrementCount(_testGuid, 1, CountType.View);
            _mockTestManager.Verify(called => called.IncrementCount(It.Is<IncrementCountCriteria>(value => value.asynch == true)), Times.Once);
        }

        [Fact]
        public void IncrementCount_Calls_TestManager_IncrementCount_WithAsynchFlag_EqualsFalse()
        {
            var aRepo = GetUnitUnderTest();
            aRepo.IncrementCount(_testGuid, 1, CountType.View, default(Guid), false);
            _mockTestManager.Verify(called => called.IncrementCount(It.Is<IncrementCountCriteria>(value => value.asynch == false)), Times.Once);
        }

        [Fact]
        public void SaveKpiResult_Calls_TestManager_SaveKpiResult_WithDefault_AsynchFlag()
        {
            var aRepo = GetUnitUnderTest();
            KeyFinancialResult result = new KeyFinancialResult();
            aRepo.SaveKpiResultData(_testGuid, 1, result, KeyResultType.Financial);
            _mockTestManager.Verify(called => called.SaveKpiResultData(It.Is<Guid>(value => value == _testGuid), It.Is<int>(value => value == 1), It.IsAny<IKeyResult>(), It.Is<KeyResultType>(value => value == KeyResultType.Financial), It.Is<bool>(value => value == true)), Times.Once);
        }

        [Fact]
        public void SaveKpiResult_Calls_TestManager_SaveKpiResult_With_AsynchFlag_EqualsFalse()
        {
            var aRepo = GetUnitUnderTest();
            KeyFinancialResult result = new KeyFinancialResult();
            aRepo.SaveKpiResultData(_testGuid, 1, result, KeyResultType.Financial, false);
            _mockTestManager.Verify(called => called.SaveKpiResultData(It.Is<Guid>(value => value == _testGuid), It.Is<int>(value => value == 1), It.IsAny<IKeyResult>(), It.Is<KeyResultType>(value => value == KeyResultType.Financial), It.Is<bool>(value => value == false)), Times.Once);
        }

        [Fact]
        public void GetActiveCachedTest_Retrieves_TestManager_ActiveCachedTests()
        {
            List<IMarketingTest> testList = new List<IMarketingTest>();
            testList.Add(new ABTest() { Title = "Test 1" });
            testList.Add(new ABTest() { Title = "Test 2" });

            var aRepo = GetUnitUnderTest();
            _mockTestManager.SetupGet(prop => prop.ActiveCachedTests).Returns(testList);
            var activeTests = aRepo.GetActiveCachedTests();
            Assert.True(activeTests.Count == 2);
            Assert.True(activeTests[0].Title == "Test 1");
            Assert.True(activeTests[1].Title == "Test 2");
        }

        [Fact]
        public void EvaluateKpis_Calls_TestManager_EvaluateKpis()
        {
            IList<IKpi> kpis = new List<IKpi>();
            string name = "Sender";
            CommerceData cData = new CommerceData() { CommerceCulture = "en" };
            kpis.Add(new Kpi() { Id = _testGuid, PreferredCommerceFormat = cData });

            var aRepo = GetUnitUnderTest();
            aRepo.EvaluateKPIs(kpis, name, new EventArgs());
            _mockTestManager.Verify(called => called.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Once);
        }

        [Fact]
        public void GetActiveTestForContent_gets_a_test_if_it_exists_for_the_content()
        {
            var aRepo = GetUnitUnderTest();
            _mockTestManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest> { new ABTest() { State = TestState.Active } });
            var aReturnValue = aRepo.GetActiveTestForContent(Guid.NewGuid());
            Assert.True(aReturnValue != null);
        }

        [Fact]
        public void GetActiveTestForContent_gets_a_test_if_it_exists_for_the_content_and_language()
        {
            var aRepo = GetUnitUnderTest();
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(new CultureInfo("en-GB"));
            _mockTestManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest> { new ABTest() { State = TestState.Active, ContentLanguage = "en-GB" } });
            var aReturnValue = aRepo.GetActiveTestForContent(Guid.NewGuid(), new CultureInfo("en-GB"));
            Assert.True(aReturnValue != null);
        }

        [Fact]
        public void GetActiveTestForContent_returns_empty_test_when_a_test_does_not_exist_for_the_content()
        {
            var aRepo = GetUnitUnderTest();
            _mockTestManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());
            var aReturnValue = aRepo.GetActiveTestForContent(Guid.NewGuid());
            Assert.True(aReturnValue.Id == Guid.Empty);
        }

        [Fact]
        public void DeleteTestForContent_calls_delete_for_every_test_associated_with_the_content_guid()
        {
            var aRepo = GetUnitUnderTest();
            var testList = new List<IMarketingTest>();

            testList.Add(new ABTest() { Id = Guid.NewGuid() });
            testList.Add(new ABTest() { Id = Guid.NewGuid() });
            testList.Add(new ABTest() { Id = Guid.NewGuid() });

            _mockTestManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(testList);
            aRepo.DeleteTestForContent(Guid.NewGuid());

            _mockTestManager.Verify(tm => tm.Delete(It.IsAny<Guid>(), null), Times.Exactly(testList.Count), "Delete was not called on all the tests in the list");
        }

        [Fact]
        public void DeleteTestForContent_calls_delete_for_every_test_associated_with_the_content_guid_and_matching_culture()
        {
            var aRepo = GetUnitUnderTest();
            //_mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(new CultureInfo("en-GB"));
            var testList = new List<IMarketingTest>();

            testList.Add(new ABTest() { Id = Guid.NewGuid(), ContentLanguage = "en-GB" });
            testList.Add(new ABTest() { Id = Guid.NewGuid(), ContentLanguage = "en-GB" });
            testList.Add(new ABTest() { Id = Guid.NewGuid(), ContentLanguage = "en-GB" });

            _mockTestManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(testList);
            aRepo.DeleteTestForContent(Guid.NewGuid(), new CultureInfo("en-GB"));

            _mockTestManager.Verify(tm => tm.Delete(It.IsAny<Guid>(), new CultureInfo("en-GB")), Times.Exactly(testList.Count), "Delete was not called on all the tests in the list");
        }

        [Fact]
        public void DeleteTestForContent_handles_guids_with_no_tests_associated_with_it_gracefully()
        {
            var aRepo = GetUnitUnderTest();
            var testList = new List<IMarketingTest>();

            _mockTestManager.Setup(tm => tm.GetTestByItemId(It.IsAny<Guid>())).Returns(testList);
            aRepo.DeleteTestForContent(Guid.NewGuid());

            _mockTestManager.Verify(tm => tm.Delete(It.IsAny<Guid>(), null), Times.Never, "Delete was called when it should not have been");
        }

        [Fact]
        public void TestResultStore_publishes_draft_content_and_republishes_published_and_sets_winner_when_published_contentid_provided()
        {
            TestResultStoreModel testResultmodel = new TestResultStoreModel
            {
                DraftContentLink = "10_101",
                PublishedContentLink = "10_100",
                TestId = Guid.NewGuid().ToString(),
                WinningContentLink = "10_100"
            };

            IMarketingTest test = new ABTest()
            {
                Variants = new List<Variant>()
                { new Variant()
                    { ItemVersion = 101,Id=Guid.Parse("f4091a7d-db88-4517-a648-a0aaedb6c213")},
                    new Variant() {ItemVersion = 100,Id=Guid.Parse("2ecb7bd5-33dd-44a5-aa05-8f82077b4896")}
                }

            };

            IContent publishedContent = new BasicContent();
            IContent draftContent = new BasicContent();

            MarketingTestingWebRepository webRepo = GetUnitUnderTest();
            _mockTestManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(test);
            _mockTestManager.Setup(
                call => call.Archive(It.IsAny<Guid>(), It.IsAny<Guid>(), null));
            _mockTestManager.Setup(
                call => call.Archive(It.IsAny<Guid>(), It.IsAny<Guid>(), null));

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                            reference => reference == ContentReference.Parse(testResultmodel.DraftContentLink))))
                .Returns(draftContent);

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                           reference => reference == ContentReference.Parse(testResultmodel.PublishedContentLink))))
               .Returns(publishedContent);

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == draftContent)))
                .Returns(new ContentReference { ID = 10, WorkID = 101 });

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == publishedContent)))
               .Returns(new ContentReference { ID = 10, WorkID = 0 });

            string aResult = webRepo.PublishWinningVariant(testResultmodel);
            Assert.True(aResult == testResultmodel.TestId);
            _mockTestResultHelper.Verify(call => call.PublishContent(draftContent), Times.Once);
            _mockTestResultHelper.Verify(call => call.PublishContent(publishedContent), Times.Once);
        }

        [Fact]
        public void TestResultStore_publishes_draft_content_and_sets_winner_when_draft_contentid_provided()
        {
            TestResultStoreModel testResultmodel = new TestResultStoreModel
            {
                DraftContentLink = "10_101",
                PublishedContentLink = "10_100",
                TestId = Guid.NewGuid().ToString(),
                WinningContentLink = "10_101"
            };

            IMarketingTest test = new ABTest()
            {
                Variants = new List<Variant>()
                { new Variant()
                    { ItemVersion = 101,Id=Guid.Parse("f4091a7d-db88-4517-a648-a0aaedb6c213")},
                    new Variant() {ItemVersion = 10,Id=Guid.Parse("2ecb7bd5-33dd-44a5-aa05-8f82077b4896")}
                }
            };

            IContent publishedContent = new BasicContent();
            IContent draftContent = new BasicContent();

            MarketingTestingWebRepository webRepo = GetUnitUnderTest();
            _mockTestManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(test);
            _mockTestManager.Setup(
                call => call.Archive(It.IsAny<Guid>(), It.IsAny<Guid>(), null));
            _mockTestManager.Setup(
                call => call.Archive(It.IsAny<Guid>(), It.IsAny<Guid>(), null));

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                            reference => reference == ContentReference.Parse(testResultmodel.DraftContentLink))))
                .Returns(draftContent);

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                           reference => reference == ContentReference.Parse(testResultmodel.PublishedContentLink))))
               .Returns(publishedContent);

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == draftContent)))
                .Returns(new ContentReference { ID = 10, WorkID = 101 });

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == publishedContent)))
               .Returns(new ContentReference { ID = 10, WorkID = 0 });

            string aResult = webRepo.PublishWinningVariant(testResultmodel);
            Assert.True(aResult == testResultmodel.TestId);
            _mockTestResultHelper.Verify(call => call.PublishContent(draftContent), Times.Once);
            _mockTestResultHelper.Verify(call => call.PublishContent(publishedContent), Times.Never);
        }

        [Fact]
        public void TestResultStore_throws_internal_server_error_when_invalid_test_ids_are_provided()
        {
            TestResultStoreModel testResultmodel = new TestResultStoreModel
            {
                DraftContentLink = "10_101",
                PublishedContentLink = "10_100",
                TestId = Guid.NewGuid().ToString(),
                WinningContentLink = "10_101"
            };

            IContent publishedContent = new BasicContent();
            IContent draftContent = new BasicContent();

            MarketingTestingWebRepository webRepo = GetUnitUnderTest();
            _mockTestManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns((IMarketingTest)null);

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                            reference => reference == ContentReference.Parse(testResultmodel.DraftContentLink))))
                .Returns(draftContent);

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                           reference => reference == ContentReference.Parse(testResultmodel.PublishedContentLink))))
               .Returns(publishedContent);

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == draftContent)))
                .Returns(new ContentReference { ID = 10, WorkID = 101 });

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == publishedContent)))
               .Returns(new ContentReference { ID = 10, WorkID = 0 });


            string aResult = webRepo.PublishWinningVariant(testResultmodel);
            Assert.True(aResult == testResultmodel.TestId);
        }

        [Fact]
        public void TestResultStore_throws_internal_server_error_when_winning_id_is_empty()
        {
            TestResultStoreModel testResultModel = new TestResultStoreModel()
            {
                DraftContentLink = string.Empty,
                PublishedContentLink = string.Empty,
                TestId = string.Empty,
                WinningContentLink = string.Empty
            };

            MarketingTestingWebRepository webRepo = GetUnitUnderTest();
            string aResult = webRepo.PublishWinningVariant(testResultModel);
            Assert.True(aResult == testResultModel.TestId);
        }

        [Fact]
        public void ConvertToMarketingTest_Converts_Test_And_Calculates_EndDate()
        {
            var webRepo = GetUnitUnderTest();
            var startDate = DateTime.Now;

            var testResultModel = new TestingStoreModel()
            {
                TestContentId = Guid.NewGuid(),
                TestDescription = "Description",
                PublishedVersion = 1,
                VariantVersion = 2,
                StartDate = startDate.ToString(),
                TestDuration = 30,
                ParticipationPercent = 100,
                KpiId = "{\"c3d26baa-dac6-4b2c-8b45-d5a05a3337d9\": 2,\"d4873545-2482-459c-b354-b6f6d5939697\": 2}",
                TestTitle = "Test Title",
                Start = false,
                ConfidenceLevel = 95,
                AutoPublishWinner = false
            };

            var test = webRepo.ConvertToMarketingTest(testResultModel);

            Assert.Equal(startDate.AddDays(30).Day, test.EndDate.Day);
        }

        [Fact]
        public void ConvertToMarketingTest_Converts_Test_With_Multiple_Kpis_With_Different_Weights()
        {
            var webRepo = GetUnitUnderTest();
            var startDate = DateTime.Now;

            var testResultModel = new TestingStoreModel()
            {
                TestContentId = Guid.NewGuid(),
                TestDescription = "Description",
                PublishedVersion = 1,
                VariantVersion = 2,
                StartDate = startDate.ToString(),
                TestDuration = 30,
                ParticipationPercent = 100,
                KpiId = "{\"c3d26baa-dac6-4b2c-8b45-d5a05a3337d9\": \"Low\",\"d4873545-2482-459c-b354-b6f6d5939697\": \"Medium\"}",
                TestTitle = "Test Title",
                Start = false,
                ConfidenceLevel = 95,
                AutoPublishWinner = false
            };

            var test = webRepo.ConvertToMarketingTest(testResultModel);

            Assert.Equal(1, test.Variants.First().KeyConversionResults.Count(w => w.Weight > .33 && w.Weight < .34));
            Assert.Equal(1, test.Variants.First().KeyConversionResults.Count(w => w.Weight > .66 && w.Weight < .68));
        }

        [Fact]
        public void Get_TestList_WithCriteria_and_Language_Returns_Correct_Tests()
        {
            ABTest test1 = new ABTest()
            {
                Title = "Test 1",
                State = TestState.Archived,
                ContentLanguage = "en-GB"
            };
            ABTest test2 = new ABTest()
            {
                Title = "Test 2",
                State = TestState.Archived,
                ContentLanguage = "en-US"
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test1, test2 };

            var criteria = new TestCriteria();
            criteria.AddFilter(new ABTestFilter()
            {
                Property = ABTestProperty.State,
                Operator = FilterOperator.And,
                Value = TestState.Archived
            });

            var webRepo = GetUnitUnderTest();
            _mockTestManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(testList);
            var results = webRepo.GetTestList(criteria, new CultureInfo("en-US"));
            Assert.True(results.Count == 1);
            Assert.True(results[0].Title == "Test 2");
        }
    }
}
