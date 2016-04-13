using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Testing.Web;
using Moq;
using Xunit;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.ServiceLocation;


namespace EPiServer.Marketing.Testing.Test.Web
{
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
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<ITestManager> _testManager;

        private List<ContentReference> _contentReferenceList;

        private Guid _noAssociatedTestGuid = Guid.Parse("b6168ed9-50d4-4609-b566-8a70ce3f5b0d");
        private Guid _associatedTestGuid = Guid.Parse("1d01f747-427e-4dd7-ad58-2449f1e28e81");
        private Guid _activeTestGuid = Guid.Parse("d9866579-ea05-4c74-a508-ab1c95766660");
        private Guid _matchingVariantId = Guid.Parse("c6c08d71-2e61-4768-8549-7bdcc43af083");
        private Guid _variantContentGuid = Guid.Parse("00000000-0000-0000-0000-7bdcc43af083");

        private TestHandler GetUnitUnderTest(List<ContentReference> contentList)
        {
            _tdc = new Mock<ITestDataCookieHelper>();
            _testManager = new Mock<ITestManager>();

            return new TestHandler(_testManager.Object, _tdc.Object, contentList);
        }
        
        [Fact]
        public void TestHandler_Page_Not_In_A_Test_Load_As_Normal()
        {
            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            
            testHandler.SwapDisabled = true;

            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(new List<IMarketingTest>());

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");

            Assert.Equal(content, args.Content); 
            Assert.Equal(content.ContentLink, args.ContentLink); 
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

            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);

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
                State = TestState.Active
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

            _testManager.Setup(call => call.GetTestByItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);
            _testManager.Setup(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(variantPage);
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);

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
                State = TestState.Active
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

            _testManager.Setup(call => call.GetTestByItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);
            _testManager.Setup(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(false);

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
                new ABTest() {OriginalItemId = _associatedTestGuid}
            };

            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Never, "Content should not have triggered IncrementCount View call");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");

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
                State = TestState.Active
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

            _testManager.Setup(call => call.GetTestByItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);
            _testManager.Setup(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
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
    }
}
