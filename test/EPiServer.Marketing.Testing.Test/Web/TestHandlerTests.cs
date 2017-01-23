using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Web;
using Moq;
using Xunit;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Test.Core;
using System.Web;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Data;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class MyLogger : ILogger
    {
        public bool ErrorCalled;
        public bool WarningCalled;
        public bool IsEnabled(Level level)
        {
            return true;
        }

        public void Log<TState, TException>(Level level, TState state, TException exception, Func<TState, TException, string> messageFormatter, Type boundaryType) where TException : Exception
        {
            if (level == Level.Error)
            {
                ErrorCalled = true;
            }
            else if (level == Level.Warning)
            {
                WarningCalled = true;
        }
    }
    }

    public class TestHandlerTests : IDisposable
    {
        public TestHandlerTests()
        {
        }

        public void Dispose()
        {
            // This works for this test framework because it creates new class instance
            // for each test. if we change frameworks you will need to put this in a tear 
            // down method or in each test.
            Assert.False(_logger.ErrorCalled, "Did you forget to mock something? Unexpected exception occured.");
        }

        private Mock<IReferenceCounter> _referenceCounter;
        private Mock<ITestDataCookieHelper> _mockTestDataCookieHelper;
        private Mock<ITestManager> _mockTestManager;
        private Mock<ITestingContextHelper> _mockContextHelper;
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<DefaultMarketingTestingEvents> _mockMarketingTestingEvents;
        private Mock<IDatabaseMode> _mockDatabaseMode;
        private MyLogger _logger = new MyLogger();

        private readonly Guid _noAssociatedTestGuid = Guid.Parse("b6168ed9-50d4-4609-b566-8a70ce3f5b0d");
        private readonly Guid _associatedTestGuid = Guid.Parse("1d01f747-427e-4dd7-ad58-2449f1e28e81");
        private readonly Guid _activeTestGuid = Guid.Parse("d9866579-ea05-4c74-a508-ab1c95766660");
        private readonly Guid _matchingVariantId = Guid.Parse("c6c08d71-2e61-4768-8549-7bdcc43af083");
        private readonly Guid _firstKpiId = Guid.Parse("ebb50f9d-8a4c-4f7f-8734-a8c31967a39a");

        private Guid _originalItemId = Guid.NewGuid();

        private TestHandler GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();
            _referenceCounter = new Mock<IReferenceCounter>();
            _mockTestDataCookieHelper = new Mock<ITestDataCookieHelper>();
            _mockTestManager = new Mock<ITestManager>();
            _mockTestManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns(new List<IMarketingTest>()
                {
                    new ABTest()
                    {
                        OriginalItemId = _originalItemId,
                        State = TestState.Active,
                        Variants = new List<Variant>() {new Variant() { ItemId = _originalItemId, ItemVersion = 2 } }
                    }
                });
            _mockTestManager.Setup(call => call.Stop(It.IsAny<Guid>()));
            _mockTestManager.Setup(call => call.Delete(It.IsAny<Guid>()));
            Variant testVariant = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 5,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid
            };
            _mockTestManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);

            _mockContextHelper = new Mock<ITestingContextHelper>();
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());

            _mockMarketingTestingEvents = new Mock<DefaultMarketingTestingEvents>();

            _mockServiceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_mockTestManager.Object);
            _mockServiceLocator.Setup(sl => sl.GetInstance<DefaultMarketingTestingEvents>())
                .Returns(_mockMarketingTestingEvents.Object);

            HttpContext.Current = new HttpContext(
               new HttpRequest(null, "http://tempuri.org", null),
               new HttpResponse(null));

            _mockServiceLocator.Setup(sl => sl.GetInstance<IReferenceCounter>())
                .Returns(_referenceCounter.Object);
            _mockServiceLocator.Setup(sl => sl.GetInstance<ITestDataCookieHelper>())
                .Returns(_mockTestDataCookieHelper.Object);
            _mockServiceLocator.Setup(sl => sl.GetInstance<ITestingContextHelper>())
                .Returns(_mockContextHelper.Object);
            _mockServiceLocator.Setup(sl => sl.GetInstance<ILogger>())
                .Returns(_logger);

            _mockDatabaseMode = new Mock<IDatabaseMode>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IDatabaseMode>())
                .Returns(_mockDatabaseMode.Object);

            ServiceLocator.SetLocator(_mockServiceLocator.Object);
            return new TestHandler(_mockServiceLocator.Object);
        }

        [Fact]
        public void TestHandler_VerifyExceptionHandler()
        {
            var th = GetUnitUnderTest();

            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();
            ContentEventArgs args = new ContentEventArgs(content);
            th.LoadedContent(new object(), args);

            // For this test we dont actually care what the exception is just that it is catching and
            // logging one.
            Assert.True(_logger.ErrorCalled, "Exception was not logged.");
            _logger.ErrorCalled = false; 
            _logger.WarningCalled = false;
        }

        [Fact]
        public void TestHandler_Page_Not_In_A_Test_Load_As_Normal()
        {
            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();

            var testHandler = GetUnitUnderTest();

            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>());
            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _mockTestDataCookieHelper.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");
            _mockTestDataCookieHelper.Verify(call => call.ExpireTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to expire cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_System_With_Valid_TestData_Cookie_And_No_Active_Test_Calls_Expire_Cookie()
        {
            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();

            var testHandler = GetUnitUnderTest();

            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());
            _mockTestManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = true, Viewed = false });
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _mockTestDataCookieHelper.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _mockTestDataCookieHelper.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(true);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);
            _mockContextHelper.Setup(call => call.GetCurrentPage()).Returns(new BasicContent());
            _mockContextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>()))
                .Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);
            _mockTestDataCookieHelper.Verify(call => call.ExpireTestDataCookie(It.IsAny<TestDataCookie>()), Times.Once(), "System should have called ExpireTestDataCookie, but did not");
        }

        [Fact]
        public void TestHandler_Disabling_The_Page_Swap_Returns_The_Published_Page()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference();

            var testHandler = GetUnitUnderTest();

            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(true);
            _mockContextHelper.Setup(call => call.GetCurrentPage()).Returns(new BasicContent());
            _mockContextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>()))
                .Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _mockTestDataCookieHelper.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _mockTestDataCookieHelper.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_Returns_A_Variant_To_User_Who_Gets_Included_In_A_Test_And_Is_Flagged_As_Seeing_The_Variant()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference() { ID = 1, WorkID = 1 };

            var pageRef = new PageReference() { ID = 2, WorkID = 2 };
            var variantPage = new PageData(pageRef);

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                KpiInstances = new List<IKpi>(),
                Variants = new List<Variant>()
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test };

            Variant testVariant = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 2,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid,
                IsPublished = false
            };

            Variant testVariant2 = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 1,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid,
                IsPublished = true
            };

            test.Variants.Add(testVariant);
            test.Variants.Add(testVariant2);

            var testHandler = GetUnitUnderTest();

            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid)).Returns(testList);
            _mockTestManager.Setup(call => call.Get(_activeTestGuid)).Returns(test);
            _mockTestManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _mockTestManager.Setup(call => call.GetVariantContent(It.IsAny<Guid>())).Returns(variantPage);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = true, Viewed = false });
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _mockTestDataCookieHelper.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _mockTestDataCookieHelper.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);
            _mockContextHelper.Setup(call => call.GetCurrentPage()).Returns(new BasicContent());
            _mockContextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>())).Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _mockTestDataCookieHelper.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should have triggered call to save cookie data");
            _mockTestDataCookieHelper.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Exactly(2), "Content should have triggered call to update cookie data");

            _mockTestManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), CountType.View, true), Times.Once, "Content should have triggered IncrementCount View call");
            Assert.Equal(variantPage, args.Content);
            Assert.Equal(variantPage.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_ContentUnderTest_New_User_Marked_As_Included_In_Test_Seeing_Published_Version_Does_Not_Get_The_Variant()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference();

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                KpiInstances = new List<IKpi>() { new Kpi() { Id = Guid.NewGuid() } },
                Variants = new List<Variant>()
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test };

            Variant testVariant = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 0,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid,
                IsPublished = true
            };

            test.Variants.Add(testVariant);

            var testHandler = GetUnitUnderTest();

            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid)).Returns(testList);
            _mockTestManager.Setup(call => call.Get(_activeTestGuid)).Returns(test);
            _mockTestManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _mockTestManager.Setup(call => call.GetVariantContent(It.IsAny<Guid>())).Returns(new PageData(content.ContentLink as PageReference));
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);
            _mockContextHelper.Setup(call => call.GetCurrentPage()).Returns(new BasicContent());
            _mockContextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>()))
                .Returns(true);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = false, Viewed = false });
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _mockTestDataCookieHelper.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _mockTestDataCookieHelper.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);

            testHandler.LoadedContent(new object(), args);

            _mockTestManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), CountType.View, true), Times.Once, "Content should have triggered IncrementCount View call");
            _mockTestDataCookieHelper.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _mockTestDataCookieHelper.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Exactly(2), "Content should have triggered call to update cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

 
        [Fact]
        public void TestHandler_User_Marked_As_Not_In_Test_Sees_The_Normal_Published_Page()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference();

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                KpiInstances = new List<IKpi>(),
                Variants = new List<Variant>()
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test };

            Variant testVariant = new Variant()
            {
                Id = Guid.Empty,
                ItemVersion = 0,
                TestId = _activeTestGuid,
                ItemId = Guid.Empty,
                IsPublished = true,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
            };

            test.Variants.Add(testVariant);

            var testHandler = GetUnitUnderTest();

            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid)).Returns(testList);
            _mockTestManager.Setup(call => call.Get(_activeTestGuid)).Returns(test);
            _mockTestManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _mockTestManager.Setup(call => call.GetVariantContent(It.IsAny<Guid>())).Returns(new PageData(content.ContentLink as PageReference));
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());
            _mockTestDataCookieHelper.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            _mockTestDataCookieHelper.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(false);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>());
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _mockTestManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), CountType.View, false), Times.Never, "Content should not have triggered IncrementCount View call");

            _mockTestDataCookieHelper.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_EvaluateKPIs_Should_Not_Be_Called_When_Cookies_Are_Marked_Converted_and_Viewed()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference();

            ContentReference testTargetLink = new ContentReference(2, 101);

            List<TestDataCookie> convertedAndViewedCookieData = new List<TestDataCookie>()
            {
                new TestDataCookie() {Converted = true, Viewed = true},
                new TestDataCookie() {Converted = true, Viewed = true},
                new TestDataCookie() {Converted = true, Viewed = true},
            };

            var testHandler = GetUnitUnderTest();

            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());
            _mockTestManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(convertedAndViewedCookieData);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = content,
                TargetLink = testTargetLink
            };

            testHandler.LoadedContent(new object(), args);
            _mockTestManager.Verify(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Never, "Test should not have called Evaluate KPIs");
        }

        [Fact]
        public void TestHandler_Cookies_Marked_Not_Converted_And_Viewed_Should_Be_Processed_By_EvaluateKPI_save_the_cookie_and_not_emit_conversion_increment()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference(); ContentReference testTargetLink = new ContentReference(2, 101);

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                KpiInstances = new List<IKpi>() { new TestKpi(_firstKpiId) { Id = _firstKpiId } }
            };

            Variant publishedVariant = new Variant()
            {
                Conversions = 5,
                Id = Guid.Parse("b7a2a5ea-5925-4fe6-b546-a31876dddafb"),
                IsWinner = false,
                ItemId = _associatedTestGuid,
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
                Id = _matchingVariantId,
                IsWinner = false,
                ItemId = _associatedTestGuid,
                ItemVersion = 190,
                Views = 50,
                TestId = test.Id,
                KeyFinancialResults = new List<KeyFinancialResult>(),
                KeyValueResults = new List<KeyValueResult>()
            };

            test.Variants = new List<Variant>() { publishedVariant, draftVariant };

            TestDataCookie testCookieOne = new TestDataCookie
            {
                Viewed = true,
                Converted = false,
                TestId = _activeTestGuid,
                AlwaysEval = false,
                TestVariantId = _matchingVariantId
            };
            testCookieOne.KpiConversionDictionary.Add(_firstKpiId, false);

            List<TestDataCookie> convertedAndViewedCookieData = new List<TestDataCookie> { testCookieOne };

            var testHandler = GetUnitUnderTest();

            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns(new List<IMarketingTest> { test });
            _mockTestManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _mockTestManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(test);
            _mockTestManager.Setup(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<object>(), It.IsAny<EventArgs>()))
                .Returns(new List<IKpiResult> { new KpiConversionResult() { KpiId = Guid.NewGuid()} });
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(convertedAndViewedCookieData);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = content,
                TargetLink = testTargetLink
            };
            testHandler.LoadedContent(new object(), args);

            _mockTestManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CountType>(), true), Times.Never, "Test should not have attempted to increment count");
        }

        [Fact]
        public void TestHandler_Cookies_Marked_Not_Converted_And_Viewed_With_ConvertedKPI_Should_Be_Processed_and_emit_conversion_increment()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference(); ContentReference testTargetLink = new ContentReference(2, 101);

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                KpiInstances = new List<IKpi>() { new TestKpi(_firstKpiId) { Id = _firstKpiId } },
                Variants = new List<Variant>() { new Variant() { Id = _matchingVariantId, ItemVersion = 5, TestId = _associatedTestGuid, ItemId = _activeTestGuid } }

            };
            TestDataCookie testCookieOne = new TestDataCookie
            {
                Viewed = true,
                Converted = false,
                TestVariantId = _matchingVariantId,
                AlwaysEval = false
            };
            testCookieOne.KpiConversionDictionary.Add(_firstKpiId, true);

            List<TestDataCookie> convertedAndViewedCookieData = new List<TestDataCookie> { testCookieOne };

            var testHandler = GetUnitUnderTest();

            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns(new List<IMarketingTest>() { test });
            _mockTestManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _mockTestManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(test);
            _mockTestManager.Setup(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<object>(), It.IsAny<EventArgs>()))
                .Returns(new List<IKpiResult> { new KpiConversionResult() { KpiId = Guid.NewGuid()} });
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(convertedAndViewedCookieData);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = content,
                TargetLink = testTargetLink
            };
            testHandler.ProxyEventHandler(new object(), args);

            _mockTestDataCookieHelper.Verify(call => call.UpdateTestDataCookie(testCookieOne), Times.AtLeast(1), "Test should have called save test data to cookie");
            _mockTestManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CountType>(), true), Times.Once, "Test should have attempted to increment count");
        }

         [Fact]
        public void TestHandler_CheckForActiveTest()
        {
            var testHandler = GetUnitUnderTest();

            // find test for published page
            Assert.Equal(1, testHandler.CheckForActiveTests(_originalItemId, 0));

            // no match for a variant
            Assert.Equal(0, testHandler.CheckForActiveTests(_originalItemId, 1));

            // find test for variant match
            Assert.Equal(1, testHandler.CheckForActiveTests(_originalItemId, 2));
        }

        [Fact]
        public void TestHandler_CheckForActiveTests_Returns_0_If_No_Tests_Found()
        {
            var testHandler = GetUnitUnderTest();
            _mockTestManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns((List<IMarketingTest>)null);
            Assert.Equal(0, testHandler.CheckForActiveTests(Guid.NewGuid(), 1));

        }

 
        [Fact]
        public void TestHandler_initProxyEventHandler_checks_ref_and_adds_one()
        {
            var testHandler = GetUnitUnderTest();
            _mockTestManager.SetupGet(g => g.ActiveCachedTests).Returns(
                new List<IMarketingTest>()
                {
                    new ABTest()
                    {
                        OriginalItemId = _originalItemId,
                        State = TestState.Active,
                        Variants = new List<Variant>() {new Variant() { ItemId = _originalItemId, ItemVersion = 2 } },
                        KpiInstances = new List<IKpi>() { new ContentComparatorKPI() { Id = Guid.NewGuid() } }
                    }
                });

            // proxyEventHandler listens for events when tests are added / removed from cache.
            Mock<IMarketingTestingEvents> testEvents = new Mock<IMarketingTestingEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>()).Returns(testEvents.Object);

            _referenceCounter.Setup(m => m.hasReference(It.IsAny<object>())).Returns(true);
            testHandler.initProxyEventHandler();

            _referenceCounter.Verify(m => m.AddReference(It.IsAny<object>()), Times.Once, "AddRef should have been called once but it wasnt.");
        }

        [Fact]
        public void TestHandler_TestRemovedFromCache_removes_reference_onSuccess()
        {
            var testHandler = GetUnitUnderTest();

            // proxyEventHandler listens for events when tests are added / removed from cache.
            Mock<IMarketingTestingEvents> testEvents = new Mock<IMarketingTestingEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>()).Returns(testEvents.Object);

            _referenceCounter.Setup(m => m.hasReference(It.IsAny<object>())).Returns(false);

            Mock<IContentEvents> ce = new Mock<IContentEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(ce.Object);

            testHandler.TestRemovedFromCache(this, new TestEventArgs(new ABTest()
            {
                OriginalItemId = _originalItemId,
                State = TestState.Active,
                Variants = new List<Variant>() { new Variant() { ItemId = _originalItemId, ItemVersion = 2 } },
                KpiInstances = new List<IKpi>() { new ContentComparatorKPI() { Id = Guid.NewGuid() } }
            }));

            _referenceCounter.Verify(m => m.RemoveReference(It.IsAny<object>()), Times.Once, "RemoveReference should be called once");
        }
    }
}