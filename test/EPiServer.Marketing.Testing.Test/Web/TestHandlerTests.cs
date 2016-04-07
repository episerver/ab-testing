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


        private TestHandler GetUnitUnderTest(List<ContentReference> contentList)
        {
            _tdc = new Mock<ITestDataCookieHelper>();
            _testManager = new Mock<ITestManager>();

            return new TestHandler(_testManager.Object, _tdc.Object, contentList);
        }

        [Fact]
        public void TestHandler_ContentNotUnderTest_and_SwapDisabled_FallsThroughProcess()
        {
            var content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            content.ContentLink = new ContentReference();
            _contentReferenceList.Add(content.ContentLink);

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
        public void TestHandler_ContentUnderTest_and_SwapDisabled_FallsThroughProcess()
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

            _tdc.Verify(call => call.GetTestDataFromCookie(It.IsAny<string>()), Times.Once(), "Content should have triggered call to get cookie data");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(), "Content should not have triggered call to update cookie data");

            Assert.Equal(content, args.Content);
            Assert.Equal(content.ContentLink, args.ContentLink);
        }

        [Fact]
        public void TestHandler_ContentUnderTest_and_SwapEnabled_NoData_SetsUpNewTestData_WithVariant_AndSwaps()
        {

            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference(6, 0);

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
            _contentReferenceList.Add(content.ContentLink);
            var testHandler = GetUnitUnderTest(_contentReferenceList);
            testHandler.SwapDisabled = false;

            _testManager.Setup(call => call.GetTestByItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);
            _testManager.Setup(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            //_tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);
            testHandler.LoadedContent(new object(), args);

            _testManager.Verify(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>()), Times.Once, "Content should have triggered Create Variant Page Cache call");
            _testManager.Verify(call => call.CreateActiveTestCache(), Times.Once, "Content should have triggered Create Active Test call");
            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Once, "Content should have triggered IncrementCount View call");

            _tdc.Verify(call => call.GetTestDataFromCookie(It.IsAny<string>()), Times.Once(),
                "Content should have triggered call to get cookie data");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Once(),
                "Content should have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Once(),
                "Content should have triggered call to update cookie data");
        }

        [Fact]
        public void TestHandler_ContentUnderTest_and_SwapEnabled_NoData_SetsUpNewTestData_WithOutVariant_AndDoesNotSwap()
        {


            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference(0, 0);
            _contentReferenceList.Add(content.ContentLink);

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

            var x = GetUnitUnderTest(_contentReferenceList);
            x.SwapDisabled = false;

            _testManager.Setup(call => call.GetTestByItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);
            _testManager.Setup(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);
            x.LoadedContent(new object(), args);

            _testManager.Verify(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>()), Times.Never, "Content should not have triggered Create Variant Page Cache call");
            _testManager.Verify(call => call.CreateActiveTestCache(), Times.Once, "Content should have triggered Create Active Test call");
            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Once, "Content should have triggered IncrementCount View call");

            _tdc.Verify(call => call.GetTestDataFromCookie(It.IsAny<string>()), Times.Once(),
                "Content should have triggered call to get cookie data");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Once(),
                "Content should have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Once(),
                "Content should have triggered call to update cookie data");

        }

        [Fact]
        public void TestHandler_ContentUnderTest_and_SwapDisabled_WithData_Count2_FallsThrough()
        {
            _contentReferenceList.Add(new ContentReference());

            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            _contentReferenceList.Add(content.ContentLink);

            List<IMarketingTest> testList = new List<IMarketingTest>()
            {
                new ABTest() {OriginalItemId = _associatedTestGuid}
            };



            var x = GetUnitUnderTest(_contentReferenceList);
            x.SwapDisabled = false;
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);

            ContentEventArgs args = new ContentEventArgs(content);
            x.LoadedContent(new object(), args);

            _testManager.Verify(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>()), Times.Never, "Content should not have triggered Create Variant Page Cache call");
            _testManager.Verify(call => call.CreateActiveTestCache(), Times.Once, "Content should have triggered Create Active Test call");
            _tdc.Verify(call => call.GetTestDataFromCookie(It.IsAny<string>()), Times.Once(),
                "Content should have triggered call to get cookie data");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(),
                "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(),
                "Content should not have triggered call to update cookie data");

        }


        [Fact]
        public void TestHandler_ContentUnderTest_and_SwapEnabled_NoData_SetsUpNewTestData_WithEmptyVariant_AndDoesNotSwap()
        {
            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference(0, 0);
            
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
            
            var x = GetUnitUnderTest(_contentReferenceList);
            x.SwapDisabled = false;

            _testManager.Setup(call => call.GetTestByItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);
            _testManager.Setup(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            _tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(false);

            ContentEventArgs args = new ContentEventArgs(content);
            x.LoadedContent(new object(), args);

            _testManager.Verify(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>()), Times.Never, "Content should not have triggered Create Variant Page Cache call");
            _testManager.Verify(call => call.CreateActiveTestCache(), Times.Once, "Content should have triggered Create Active Test call");
            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), CountType.View), Times.Never, "Content should have triggered IncrementCount View call");

            _tdc.Verify(call => call.GetTestDataFromCookie(It.IsAny<string>()), Times.Once(),
                "Content should have triggered call to get cookie data");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Once(),
                "Content should have triggered call to save cookie data");
        }
    }
}
