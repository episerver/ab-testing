using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Test.Core;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.ClientKPI;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class MyLogger : ILogger
    {
        public bool ErrorCalled;
        public bool WarningCalled;
        public bool DebugCalled;
        public bool InformationCalled;
        public string Message { get; set; }

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
            else if (level == Level.Debug)
            {
                DebugCalled = true;
            }
            else if (level == Level.Information)
            {
                InformationCalled = true;
            }

            Message = messageFormatter.Invoke(state, exception);
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
        private Mock<IMarketingTestingWebRepository> _mockMarketingTestingWebRepository;
        private Mock<ITestingContextHelper> _mockContextHelper;
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<DefaultMarketingTestingEvents> _mockMarketingTestingEvents;
        private Mock<IDatabaseMode> _mockDatabaseMode;
        private MyLogger _logger = new MyLogger();
        private Mock<IClientKpiInjector> _clientKpiInjector;
        private Mock<IContentEvents> _contentEvents;
        private Mock<IHttpContextHelper> _mockHttpContextHelper;
        private Mock<IEpiserverHelper> _mockEpiserverHelper;

        private readonly Guid _noAssociatedTestGuid = Guid.Parse("b6168ed9-50d4-4609-b566-8a70ce3f5b0d");
        private readonly Guid _associatedTestGuid = Guid.Parse("1d01f747-427e-4dd7-ad58-2449f1e28e81");
        private readonly Guid _activeTestGuid = Guid.Parse("d9866579-ea05-4c74-a508-ab1c95766660");
        private readonly Guid _matchingVariantId = Guid.Parse("c6c08d71-2e61-4768-8549-7bdcc43af083");
        private readonly Guid _firstKpiId = Guid.Parse("ebb50f9d-8a4c-4f7f-8734-a8c31967a39a");
        private FakeMarketingTestingEvents _testEvents;

        private Guid _originalItemId = Guid.NewGuid();

        private TestHandler GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();
            _referenceCounter = new Mock<IReferenceCounter>();
            _mockTestDataCookieHelper = new Mock<ITestDataCookieHelper>();
            _mockMarketingTestingWebRepository = new Mock<IMarketingTestingWebRepository>();
            _mockEpiserverHelper = new Mock<IEpiserverHelper>();

            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>()
                {
                    new ABTest()
                    {
                        OriginalItemId = _originalItemId,
                        State = TestState.Active,
                        ContentLanguage = "en-GB",
                        Variants = new List<Variant>() {new Variant() { ItemId = _originalItemId, ItemVersion = 2 } }
                    }
                });

            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTests()).Returns(new List<IMarketingTest> { });

            Variant testVariant = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 5,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid
            };
            _mockMarketingTestingWebRepository.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);

            _mockContextHelper = new Mock<ITestingContextHelper>();
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(new TestDataCookie());

            _mockMarketingTestingEvents = new Mock<DefaultMarketingTestingEvents>();

            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingWebRepository>()).Returns(_mockMarketingTestingWebRepository.Object);
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
            _mockServiceLocator.Setup(sl => sl.GetInstance<IEpiserverHelper>()).Returns(_mockEpiserverHelper.Object);

            _mockDatabaseMode = new Mock<IDatabaseMode>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IDatabaseMode>())
                .Returns(_mockDatabaseMode.Object);
            _clientKpiInjector = new Mock<IClientKpiInjector>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IClientKpiInjector>())
                .Returns(_clientKpiInjector.Object);

            _contentEvents = new Mock<IContentEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(_contentEvents.Object);
            _mockHttpContextHelper = new Mock<IHttpContextHelper>();
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = true };

            // proxyEventHandler listens for events when tests are added / removed from cache.
            _testEvents = new FakeMarketingTestingEvents();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>()).Returns(_testEvents);

            return new TestHandler(_mockServiceLocator.Object, _mockHttpContextHelper.Object);
        }

        [Fact]
        public void VerifyExceptionHandler()
        {
            var th = GetUnitUnderTest();

            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();
            ContentEventArgs args = new ContentEventArgs(content);
            th.LoadedContent(new object(), args);

            // For this test we dont actually care what the exception is just that it is catching and
            // logging one.
            Assert.True(_logger.DebugCalled, "Exception was not logged.");
            _logger.ErrorCalled = false;
            _logger.WarningCalled = false;
            _logger.DebugCalled = false;
        }

        [Fact]
        public void Page_Not_In_A_Test_Load_As_Normal()
        {
            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();

            var testHandler = GetUnitUnderTest();

            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>());
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _mockTestDataCookieHelper.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");
            _mockTestDataCookieHelper.Verify(call => call.ExpireTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to expire cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        #region EnabelingABTesting
        [Fact]
        public void Contructor_DisablesABTesting_If_Disabled_In_Config()
        {
            GetUnitUnderTest();
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = false };
            AdminConfigTestSettings._currentSettings._serviceLocator = _mockServiceLocator.Object;
            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            var fakeContentEvents = new FakeContentEvents();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(fakeContentEvents);
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTests())
                .Returns(new List<IMarketingTest>());
            Mock<IMarketingTestingEvents> testEvents = new Mock<IMarketingTestingEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>()).Returns(testEvents.Object);

            var testHandler = new TestHandler();
            Assert.Equal(0, fakeContentEvents.LoadedContentCounter);
            Assert.Equal(0, fakeContentEvents.LoadedChildrenCounter);
        }

        [Fact]
        public void EnableABTesting_AddsLoadedContentListeners_OnlyOnce()
        {
            GetUnitUnderTest();
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = false };
            AdminConfigTestSettings._currentSettings._serviceLocator = _mockServiceLocator.Object;
            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            var contentEvents = new FakeContentEvents();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(contentEvents);
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTests())
                .Returns(new List<IMarketingTest>());
            Mock<IMarketingTestingEvents> testEvents = new Mock<IMarketingTestingEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>()).Returns(testEvents.Object);

            var testHandler = new TestHandler();
            Assert.Equal(0, contentEvents.LoadedContentCounter);
            Assert.Equal(0, contentEvents.LoadedChildrenCounter);

            testHandler.EnableABTesting();
            testHandler.EnableABTesting();

            Assert.Equal(1, contentEvents.LoadedContentCounter);
            Assert.Equal(1, contentEvents.LoadedChildrenCounter);
        }

        [Fact]
        public void EnableABTesting_LogsInformationMessage()
        {
            var testHandler = GetUnitUnderTest();
            testHandler.EnableABTesting();

            Assert.True(_logger.InformationCalled);
            Assert.Contains("enabled", _logger.Message);
        }

        [Fact]
        public void EnableABTesting_AddsLoadedContentListeners()
        {
            GetUnitUnderTest();
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = false };
            AdminConfigTestSettings._currentSettings._serviceLocator = _mockServiceLocator.Object;
            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            var contentEvents = new FakeContentEvents();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(contentEvents);
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTests())
                .Returns(new List<IMarketingTest>());
            Mock<IMarketingTestingEvents> testEvents = new Mock<IMarketingTestingEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>()).Returns(testEvents.Object);

            var testHandler = new TestHandler();
            Assert.Equal(0, contentEvents.LoadedContentCounter);
            Assert.Equal(0, contentEvents.LoadedChildrenCounter);

            testHandler.EnableABTesting();
            Assert.Equal(1, contentEvents.LoadedContentCounter);
            Assert.Equal(1, contentEvents.LoadedChildrenCounter);
        }

        [Fact]
        public void EnableABTesting_EnablesProxyEventHandlers()
        {
            GetUnitUnderTest();
            AdminConfigTestSettings._currentSettings = new AdminConfigTestSettings() { IsEnabled = false };
            AdminConfigTestSettings._currentSettings._serviceLocator = _mockServiceLocator.Object;
            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            var contentEvents = new FakeContentEvents();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(contentEvents);
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTests())
                .Returns(new List<IMarketingTest>());

            var testHandler = new TestHandler();
            testHandler.EnableABTesting();

            Assert.Equal(1, _testEvents.TestAddedToCacheCounter);
            Assert.Equal(1, _testEvents.TestRemovedFromCacheCounter);
        }

        [Fact]
        public void DisableABTesting_RemovesLoadedContentListeners()
        {
            GetUnitUnderTest();

            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            var contentEvents = new FakeContentEvents();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(contentEvents);
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTests())
                .Returns(new List<IMarketingTest>());
            Mock<IMarketingTestingEvents> testEvents = new Mock<IMarketingTestingEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>()).Returns(testEvents.Object);

            var testHandler = new TestHandler();
            testHandler.EnableABTesting();
            Assert.Equal(1, contentEvents.LoadedContentCounter);
            Assert.Equal(1, contentEvents.LoadedChildrenCounter);

            testHandler.DisableABTesting();
            Assert.Equal(0, contentEvents.LoadedContentCounter);
            Assert.Equal(0, contentEvents.LoadedChildrenCounter);
        }

        [Fact]
        public void DisableABTesting_LogsInformationMessage()
        {
            var testHandler = GetUnitUnderTest();
            testHandler.DisableABTesting();

            Assert.True(_logger.InformationCalled);
            Assert.Contains("disabled", _logger.Message);
        }

        [Fact]
        public void DisableABTesting_DisablesProxyEventHandlers()
        {
            GetUnitUnderTest();

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                ContentLanguage = "en-GB",
                KpiInstances = new List<IKpi>(),
                Variants = new List<Variant>()
            };

            List<IMarketingTest> testList = new List<IMarketingTest>() { test };

            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            var contentEvents = new FakeContentEvents();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(contentEvents);
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTests())
                .Returns(testList);

            var testHandler = new TestHandler();
            testHandler.EnableABTesting();
            testHandler.DisableABTesting();

            Assert.Equal(0, _testEvents.TestAddedToCacheCounter);
            Assert.Equal(1, _testEvents.TestRemovedFromCacheCounter);
        }

        #endregion

        [Fact]
        public void Disabling_The_Page_Swap_Returns_The_Published_Page()
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
        public void Returns_A_Variant_To_User_Who_Gets_Included_In_A_Test_And_Is_Flagged_As_Seeing_The_Variant()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference() { ID = 1, WorkID = 1 };
            CultureInfo testCulture = CultureInfo.GetCultureInfo("en-GB");

            var pageRef = new PageReference() { ID = 2, WorkID = 2 };
            var variantPage = new PageData(pageRef);

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                ContentLanguage = "en-GB",
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
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(testCulture);
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid, testCulture)).Returns(testList);
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(_activeTestGuid, It.IsAny<bool>())).Returns(test);
            _mockMarketingTestingWebRepository.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _mockMarketingTestingWebRepository.Setup(call => call.GetVariantContent(It.IsAny<Guid>(), testCulture)).Returns(variantPage);
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(test);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = true, Viewed = false });
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

            _mockMarketingTestingWebRepository.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), It.Is<CountType>(value => value == CountType.View), It.IsAny<Guid>(), It.Is<bool>(value => value == true)), Times.Once, "Content should have triggered IncrementCount View call");
            Assert.Equal(variantPage, args.Content);
            Assert.Equal(variantPage.ContentLink, args.ContentLink);
        }

        [Fact]
        public void ContentUnderTest_New_User_Marked_As_Included_In_Test_Seeing_Published_Version_Does_Not_Get_The_Variant()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference();
            CultureInfo testCulture = CultureInfo.GetCultureInfo("en-GB");

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                KpiInstances = new List<IKpi>() { new Kpi() { Id = Guid.NewGuid() } },
                Variants = new List<Variant>(),
                ContentLanguage = "en-GB"
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
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(testCulture);
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid, testCulture)).Returns(testList);
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(_activeTestGuid, It.IsAny<bool>())).Returns(test);
            _mockMarketingTestingWebRepository.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _mockMarketingTestingWebRepository.Setup(call => call.GetVariantContent(It.IsAny<Guid>())).Returns(new PageData(content.ContentLink as PageReference));
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(test);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);
            _mockContextHelper.Setup(call => call.GetCurrentPage()).Returns(new BasicContent());
            _mockContextHelper.Setup(call => call.IsRequestedContent(It.IsAny<IContent>()))
                .Returns(true);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(new TestDataCookie { Converted = false, ShowVariant = false, Viewed = false });
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _mockTestDataCookieHelper.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _mockTestDataCookieHelper.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);

            testHandler.LoadedContent(new object(), args);

            _mockMarketingTestingWebRepository.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), It.Is<CountType>(value => value == CountType.View), It.IsAny<Guid>(), It.Is<bool>(value => value == true)), Times.Once, "Content should have triggered IncrementCount View call");
            _mockTestDataCookieHelper.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _mockTestDataCookieHelper.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Exactly(2), "Content should have triggered call to update cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }


        [Fact]
        public void User_Marked_As_Not_In_Test_Sees_The_Normal_Published_Page()
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

            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(_associatedTestGuid)).Returns(testList);
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(_activeTestGuid, It.IsAny<bool>())).Returns(test);
            _mockMarketingTestingWebRepository.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _mockMarketingTestingWebRepository.Setup(call => call.GetVariantContent(It.IsAny<Guid>())).Returns(new PageData(content.ContentLink as PageReference));
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(test);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(new TestDataCookie());
            _mockTestDataCookieHelper.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            _mockTestDataCookieHelper.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(false);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>());
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _mockMarketingTestingWebRepository.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CountType>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never, "Content should not have triggered IncrementCount View call");

            _mockTestDataCookieHelper.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void EvaluateKPIs_Should_Not_Be_Called_When_Cookies_Are_Marked_Converted_and_Viewed()
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

            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns(new List<IMarketingTest>());
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(convertedAndViewedCookieData);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = content,
                TargetLink = testTargetLink
            };

            testHandler.LoadedContent(new object(), args);
            _mockMarketingTestingWebRepository.Verify(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Never, "Test should not have called Evaluate KPIs");
        }

        [Fact]
        public void Cookies_Marked_Not_Converted_And_Viewed_Should_Be_Processed_By_EvaluateKPI_save_the_cookie_and_not_emit_conversion_increment()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference(); ContentReference testTargetLink = new ContentReference(2, 101);

            Variant testVariant = new Variant()
            {
                Id = _matchingVariantId,
                ItemVersion = 0,
                TestId = _activeTestGuid,
                ItemId = Guid.NewGuid(),
                IsPublished = false
            };

            IMarketingTest test = new ABTest()
            {
                Id = _activeTestGuid,
                OriginalItemId = _associatedTestGuid,
                State = TestState.Active,
                KpiInstances = new List<IKpi>() { new TestKpi(_firstKpiId) { Id = _firstKpiId } },
                Variants = new List<Variant>()
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

            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns(new List<IMarketingTest> { test });
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(test);
            _mockMarketingTestingWebRepository.Setup(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<object>(), It.IsAny<EventArgs>()))
                .Returns(new List<IKpiResult> { new KpiConversionResult() { KpiId = Guid.NewGuid() } });
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(convertedAndViewedCookieData);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = content,
                TargetLink = testTargetLink
            };
            testHandler.LoadedContent(new object(), args);

            _mockMarketingTestingWebRepository.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CountType>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never, "Test should not have attempted to increment count");
        }

        [Fact]
        public void Cookies_Marked_Not_Converted_And_Viewed_With_ConvertedKPI_Should_Be_Processed_and_emit_conversion_increment()
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

            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>()))
                .Returns(new List<IMarketingTest>() { test });
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(test);
            _mockMarketingTestingWebRepository.Setup(call => call.EvaluateKPIs(It.IsAny<List<IKpi>>(), It.IsAny<object>(), It.IsAny<EventArgs>()))
                .Returns(new List<IKpiResult> { new KpiConversionResult() { HasConverted = true, KpiId = Guid.NewGuid() } });
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(convertedAndViewedCookieData);
            _mockContextHelper.Setup(call => call.SwapDisabled(It.IsAny<ContentEventArgs>())).Returns(false);
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));
            ContentEventArgs args = new ContentEventArgs(new ContentReference(2, 100))
            {
                Content = content,
                TargetLink = testTargetLink
            };
            testHandler.ProxyEventHandler(new object(), args);

            _mockTestDataCookieHelper.Verify(call => call.UpdateTestDataCookie(testCookieOne), Times.AtLeast(1), "Test should have called save test data to cookie");
            _mockMarketingTestingWebRepository.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CountType>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once, "Test should have attempted to increment count");
        }

        [Fact]
        public void DeleteActiveTests()
        {
            var testHandler = GetUnitUnderTest();
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));

            // find test for published page
            Assert.Equal(1, testHandler.DeleteActiveTests(_originalItemId, 0));

            // no match for a variant
            Assert.Equal(0, testHandler.DeleteActiveTests(_originalItemId, 1));

            // find test for variant match
            Assert.Equal(1, testHandler.DeleteActiveTests(_originalItemId, 2));
        }

        [Fact]
        public void DeleteActiveTests_Returns_0_If_No_Tests_Found()
        {
            var testHandler = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>()))
                .Returns((List<IMarketingTest>)null);
            Assert.Equal(0, testHandler.DeleteActiveTests(Guid.NewGuid(), 1));
        }


        [Fact]
        public void EnableProxyEventHandler_checks_ref_and_adds_one()
        {
            var testHandler = GetUnitUnderTest();
            var expectedTests = new List<IMarketingTest>()
            {
                new ABTest()
                {
                    OriginalItemId = _originalItemId,
                    State = TestState.Active,
                    Variants = new List<Variant>() {new Variant() { ItemId = _originalItemId, ItemVersion = 2 } },
                    KpiInstances = new List<IKpi>() { new ContentComparatorKPI(_mockServiceLocator.Object,Guid.NewGuid()) }
                }
            };

            _mockMarketingTestingWebRepository.Setup(r => r.GetActiveTests()).Returns(expectedTests);
            _referenceCounter.Setup(m => m.hasReference(It.IsAny<object>())).Returns(true);
            testHandler.enableProxyEventHandler();

            _referenceCounter.Verify(m => m.AddReference(It.IsAny<object>()), Times.Once, "AddRef should have been called once but it wasnt.");
            Assert.Equal(1, _testEvents.TestAddedToCacheCounter);
            Assert.Equal(1, _testEvents.TestRemovedFromCacheCounter);
        }

        [Fact]
        public void DisableProxyEventHandler_checks_ref_and_removes_one()
        {
            var testHandler = GetUnitUnderTest();
            var expectedTests = new List<IMarketingTest>()
            {
                new ABTest()
                {
                    OriginalItemId = _originalItemId,
                    State = TestState.Active,
                    Variants = new List<Variant>() {new Variant() { ItemId = _originalItemId, ItemVersion = 2 } },
                    KpiInstances = new List<IKpi>() { new ContentComparatorKPI(_mockServiceLocator.Object,Guid.NewGuid()) }
                }
            };

            _mockMarketingTestingWebRepository.Setup(r => r.GetActiveTests()).Returns(expectedTests);

            _referenceCounter.Setup(m => m.hasReference(It.IsAny<object>())).Returns(true);

            testHandler.disableProxyEventHandler();

            _referenceCounter.Verify(m => m.RemoveReference(It.IsAny<object>()), Times.Once, "RemoveRef should have been called once but it wasnt.");
            Assert.Equal(0, _testEvents.TestAddedToCacheCounter);
            Assert.Equal(0, _testEvents.TestRemovedFromCacheCounter);
        }

        [Fact]
        public void TestRemovedFromCache_removes_reference_onSuccess()
        {
            var testHandler = GetUnitUnderTest();

            // proxyEventHandler listens for events when tests are added / removed from cache.
            Mock<IMarketingTestingEvents> testEvents = new Mock<IMarketingTestingEvents>();
            _mockServiceLocator.Setup(sl => sl.GetInstance<IMarketingTestingEvents>()).Returns(testEvents.Object);

            _referenceCounter.Setup(m => m.hasReference(It.IsAny<object>())).Returns(false);

            testHandler.TestRemovedFromCache(this, new TestEventArgs(new ABTest()
            {
                OriginalItemId = _originalItemId,
                State = TestState.Active,
                Variants = new List<Variant>() { new Variant() { ItemId = _originalItemId, ItemVersion = 2 } },
                KpiInstances = new List<IKpi>() { (new Mock<IKpi>()).Object }
            }));

            _referenceCounter.Verify(m => m.RemoveReference(It.IsAny<object>()), Times.Once, "RemoveReference should be called once");
        }

        [Fact]
        public void LoadedChildren()
        {
            var th = GetUnitUnderTest();

            _mockMarketingTestingWebRepository.Setup(call => call.GetVariantContent(It.IsAny<Guid>())).Returns(new BasicContent());

            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookies()).Returns(new List<TestDataCookie>() { new TestDataCookie() });
            _mockTestDataCookieHelper.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(true);
            _mockTestDataCookieHelper.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(true);
            _mockTestDataCookieHelper.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(new TestDataCookie() { ShowVariant = true });

            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));

            var t = new ContentReference(1, 3);
            var args = new ChildrenEventArgs(t, new List<IContent>() { new BasicContent() });
            th.LoadedChildren(new object(), args);
        }
    }
}