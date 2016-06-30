using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
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
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Marketing.Testing.Test.Core;
using Xunit.Sdk;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class MyLogger : ILogger
    {
        public bool errorCalled = false;
        public bool IsEnabled(Level level)
        {
            return true;
        }

        public void Log<TState, TException>(Level level, TState state, TException exception, Func<TState, TException, string> messageFormatter, Type boundaryType) where TException : Exception
        {
            if (level == Level.Error)
            {
                errorCalled = true;
            }
        }
    }

    public class TestHandlerTests : IDisposable
    {
        public TestHandlerTests()
        {
            _contentReferenceList = new List<ContentReference>();
        }

        public void Dispose()
        {
            _contentReferenceList.Clear();
        }

        private Mock<ITestDataCookieHelper> _tdc;
        private Mock<ITestManager> _testManager;
        private Mock<ITestingContextHelper> _contextHelper;
        private MyLogger _logger;

        private readonly List<ContentReference> _contentReferenceList;

        private readonly Guid _noAssociatedTestGuid = Guid.Parse("b6168ed9-50d4-4609-b566-8a70ce3f5b0d");
        private readonly Guid _associatedTestGuid = Guid.Parse("1d01f747-427e-4dd7-ad58-2449f1e28e81");
        private readonly Guid _activeTestGuid = Guid.Parse("d9866579-ea05-4c74-a508-ab1c95766660");
        private readonly Guid _matchingVariantId = Guid.Parse("c6c08d71-2e61-4768-8549-7bdcc43af083");
        private readonly Guid _firstKpiId = Guid.Parse("ebb50f9d-8a4c-4f7f-8734-a8c31967a39a");
        private readonly Guid _secondKpiId = Guid.Parse("64d9e743-0c74-4098-b23f-a7046b7d7be8");

        private TestHandler GetUnitUnderTest(List<ContentReference> contentList)
        {
            _logger = new MyLogger();

            _tdc = new Mock<ITestDataCookieHelper>();
            _testManager = new Mock<ITestManager>();
            _testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _contextHelper = new Mock<ITestingContextHelper>();
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());

            return new TestHandler(_testManager.Object, _tdc.Object, contentList, _contextHelper.Object, _logger);
        }

        [Fact]
        public void TestHandler_VerifyExceptionHandler()
        {
            var th = GetUnitUnderTest(_contentReferenceList);

            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();
            ContentEventArgs args = new ContentEventArgs(content);
            th.LoadedContent(new object(), args);

            // For this test we dont actually care what the exception is just that it is catching and
            // logging one.
            Assert.True(_logger.errorCalled, "Exception was not logged.");
        }

        [Fact]
        public void TestHandler_Null_Content_Argument_Does_Not_Attempt_To_Process_ContentSwitching()
        {
            BasicContent content = null;

            var testHandler = GetUnitUnderTest(_contentReferenceList);

            testHandler.SwapDisabled = false;

            _contextHelper.Setup(call => call.GetCurrentPageFromUrl()).Returns(new BasicContent());
            _contextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>(), It.IsAny<IContent>()))
                .Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            //if cookie managemenet is attempted the null content made it through.    
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");
        }

        [Fact]
        public void TestHandler_Page_Not_In_A_Test_Load_As_Normal()
        {
            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();

            var testHandler = GetUnitUnderTest(_contentReferenceList);

            testHandler.SwapDisabled = true;

            _contextHelper.Setup(call => call.GetCurrentPageFromUrl()).Returns(new BasicContent());
            _contextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>(), It.IsAny<IContent>()))
                .Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_System_With_Valid_TestData_Cookie_And_No_Active_Test_Calls_Expire_Cookie()
        {
            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());
            _testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = true, Viewed = false });
            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(true);
            _contextHelper.Setup(call => call.GetCurrentPageFromUrl()).Returns(new BasicContent());
            _contextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>(), It.IsAny<IContent>()))
                .Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);
            _tdc.Verify(call => call.ExpireTestDataCookie(It.IsAny<TestDataCookie>()), Times.Once(), "System should have called ExpireTestDataCookie, but did not");
        }

        [Fact]
        public void TestHandler_Disabling_The_Page_Swap_Returns_The_Published_Page()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference();
            _contentReferenceList.Add(content.ContentLink);

            List<IMarketingTest> testList = new List<IMarketingTest>()
            {
                new ABTest() {OriginalItemId = _associatedTestGuid}
            };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = true;

            _contextHelper.Setup(call => call.GetCurrentPageFromUrl()).Returns(new BasicContent());
            _contextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>(), It.IsAny<IContent>()))
                .Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");

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
                KpiInstances = new List<IKpi>()
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test };

            Variant testVariant = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 5,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid
            };
            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.GetVariantPageData(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(variantPage);
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = true, Viewed = false });
            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            _contextHelper.Setup(call => call.GetCurrentPageFromUrl()).Returns(new BasicContent());
            _contextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>(), It.IsAny<IContent>()))
                .Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Once(), "Content should have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Once(), "Content should have triggered call to update cookie data");

            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Once, "Content should have triggered IncrementCount View call");
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
                KpiInstances = new List<IKpi>() { new Kpi() { Id = Guid.NewGuid() } }
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test };

            Variant testVariant = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 0,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid
            };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.GetVariantPageData(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
            _contextHelper.Setup(call => call.GetCurrentPageFromUrl()).Returns(new BasicContent());
            _contextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>(), It.IsAny<IContent>()))
                .Returns(true);
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = false, Viewed = false });
            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Once, "Content should have triggered IncrementCount View call");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Once(), "Content should have triggered call to update cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_ContentUnderTest_Returning_User_Included_In_A_Test_Marked_As_Seeing_Published_Gets_The_Published_Page_But_Does_Not_Count_As_A_View()
        {
            _contentReferenceList.Add(new ContentReference());

            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference();
            _contentReferenceList.Add(content.ContentLink);

            List<IMarketingTest> testList = new List<IMarketingTest>()
            {
                new ABTest() {
                    OriginalItemId = _associatedTestGuid,
                    KpiInstances = new List<IKpi>()
                }
            };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;
            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns(testList);
            _testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(testList);
            _contextHelper.Setup(call => call.GetCurrentPageFromUrl()).Returns(new BasicContent());
            _contextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>(), It.IsAny<IContent>()))
                .Returns(true);
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = false, Viewed = true });
            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Never, "Content should not have triggered IncrementCount View call");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_ContentUnderTest_Returning_User_Included_In_A_Test_Marked_As_Seeing_Variant_Gets_The_Variant_Page_But_Does_Not_Count_As_A_View()
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
                KpiInstances = new List<IKpi>()
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test };

            Variant testVariant = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 5,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid
            };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.GetVariantPageData(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(variantPage);
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = true, Viewed = true });
            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(true);
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _contextHelper.Setup(call => call.GetCurrentPageFromUrl()).Returns(new BasicContent());
            _contextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>(), It.IsAny<IContent>()))
                .Returns(true);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Never, "Content should  not have triggered IncrementCount View call");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");
            Assert.Equal(variantPage, args.Content);
            Assert.Equal(variantPage.ContentLink, args.ContentLink);
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
                KpiInstances = new List<IKpi>()
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test };

            Variant testVariant = new Variant()
            {
                Id = Guid.Empty,
                ItemVersion = 0,
                TestId = _activeTestGuid,
                ItemId = Guid.Empty
            };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.GetVariantPageData(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Never, "Content should not have triggered IncrementCount View call");

            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_EvaluateKPIs_Should_Not_Be_Called_When_Cookies_Are_Marked_Converted_and_Viewed()
        {
            BasicContent nullContent = null;
            ContentReference testTargetLink = new ContentReference(2, 101);

            List<TestDataCookie> convertedAndViewedCookieData = new List<TestDataCookie>()
            {
                new TestDataCookie() {Converted = true, Viewed = true},
                new TestDataCookie() {Converted = true, Viewed = true},
                new TestDataCookie() {Converted = true, Viewed = true},
            };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());
            _testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());

            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(convertedAndViewedCookieData);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = nullContent,
                TargetLink = testTargetLink
            };

            testHandler.LoadedContent(new object(), args);
            _testManager.Verify(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<IContent>()), Times.Never, "Test should not have called Evaluate KPIs");
        }

        [Fact]
        public void TestHandler_Cookies_Marked_Not_Converted_And_Viewed_Should_Be_Processed_By_EvaluateKPI_save_the_cookie_and_not_emit_conversion_increment()
        {
            BasicContent nullContent = null;
            ContentReference testTargetLink = new ContentReference(2, 101);

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                KpiInstances = new List<IKpi>() { new TestKpi(_firstKpiId) { Id = _firstKpiId } }
            };
            TestDataCookie testCookieOne = new TestDataCookie
            {
                Viewed = true,
                Converted = false
            };
            testCookieOne.KpiConversionDictionary.Add(_firstKpiId, false);

            List<TestDataCookie> convertedAndViewedCookieData = new List<TestDataCookie> { testCookieOne };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns(new List<IMarketingTest>());
            _testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _testManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(test);
            _testManager.Setup(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<IContent>()))
                .Returns(new List<Guid> { Guid.NewGuid() });

            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(convertedAndViewedCookieData);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = nullContent,
                TargetLink = testTargetLink
            };
            testHandler.LoadedContent(new object(), args);

            _tdc.Verify(call => call.SaveTestDataToCookie(testCookieOne), Times.Once, "Test should have called save test data to cookie");
            _testManager.Verify(call => call.EmitUpdateCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CountType>()), Times.Never, "Test should not have attempted to increment count");
        }

        [Fact]
        public void TestHandler_Cookies_Marked_Not_Converted_And_Viewed_With_ConvertedKPI_Should_Be_Processed_and_emit_conversion_increment()
        {
            BasicContent nullContent = null;
            ContentReference testTargetLink = new ContentReference(2, 101);

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
                TestVariantId = _matchingVariantId
            };
            testCookieOne.KpiConversionDictionary.Add(_firstKpiId, true);

            List<TestDataCookie> convertedAndViewedCookieData = new List<TestDataCookie> { testCookieOne };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns(new List<IMarketingTest>());
            _testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _testManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(test);
            _testManager.Setup(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<IContent>()))
                .Returns(new List<Guid> { Guid.NewGuid() });

            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(convertedAndViewedCookieData);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = nullContent,
                TargetLink = testTargetLink
            };
            testHandler.LoadedContent(new object(), args);

            _tdc.Verify(call => call.SaveTestDataToCookie(testCookieOne), Times.Once, "Test should have called save test data to cookie");
            _testManager.Verify(call => call.EmitUpdateCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CountType>()), Times.Once, "Test should have attempted to increment count");
        }

        [Fact]
        public void TestHandler_EvaluateKpi_Catch_Throws_TestNotFound_Exception()
        {
            BasicContent nullContent = null;
            ContentReference testTargetLink = new ContentReference(2, 101);

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
                TestVariantId = _matchingVariantId
            };
            testCookieOne.KpiConversionDictionary.Add(_firstKpiId, true);

            List<TestDataCookie> convertedAndViewedCookieData = new List<TestDataCookie> { testCookieOne };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns(new List<IMarketingTest>());
            _testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _testManager.Setup(call => call.Get(It.IsAny<Guid>())).Throws(new TestNotFoundException());
            _testManager.Setup(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<IContent>()))
                .Returns(new List<Guid> { Guid.NewGuid() });

            _tdc.Setup(call => call.getTestDataFromCookies()).Returns(convertedAndViewedCookieData);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = nullContent,
                TargetLink = testTargetLink
            };
            testHandler.LoadedContent(new object(), args);
            _tdc.Verify(call => call.ExpireTestDataCookie(It.IsAny<TestDataCookie>()), Times.Once, "Expected expire test data cookie to be called");
        }
    }
}