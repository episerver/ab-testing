using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EPiServer.Marketing.Testing.Web;
using Moq;
using Xunit;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.ServiceLocation;


namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestHandlerTests
    {
        private Mock<ITestDataCookieHelper> _tdc;
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<ITestManager> _testManager;

        private Guid _noAssociatedTestGuid = Guid.Parse("b6168ed9-50d4-4609-b566-8a70ce3f5b0d");
        private Guid _associatedTestGuid = Guid.Parse("1d01f747-427e-4dd7-ad58-2449f1e28e81");
        private Guid _activeTestGuid = Guid.Parse("d9866579-ea05-4c74-a508-ab1c95766660");
        private Guid _matchingVariantId = Guid.Parse("c6c08d71-2e61-4768-8549-7bdcc43af083");
        private Guid _alternateVariantId = Guid.Parse("32edc607-2edd-460d-b3b9-bcaf95ef0234");


        private TestHandler GetUnitUnderTest(List<ContentReference> contentList, bool swapState, TestDataCookie testData)
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testManager = new Mock<ITestManager>();
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testManager.Object);

            _tdc = new Mock<ITestDataCookieHelper>();
            _tdc.Setup(td => td.GetTestDataFromCookie(It.Is<string>(x => x.Contains(_noAssociatedTestGuid.ToString()))))
                .Returns(new TestDataCookie());

            return new TestHandler(_serviceLocator.Object, _tdc.Object, contentList, swapState, testData);
        }

        [Fact]
        public void TestHandler_ContentNotUnderTest_and_SwapDisabled_FallsThroughProcess()
        {
            List<ContentReference> contentRef = new List<ContentReference>();
            TestDataCookie testData = new TestDataCookie();
            IContent content = new BasicContent();
            content.ContentGuid = _noAssociatedTestGuid;
            contentRef.Add(content.ContentLink);

            var x = GetUnitUnderTest(contentRef, true, testData);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(new List<IMarketingTest>());

            ContentEventArgs args = new ContentEventArgs(content);
            x.LoadedContent(new object(), args);

            _testManager.Verify(call=>call.CreateVariantPageDataCache(It.IsAny<Guid>(),It.IsAny<List<ContentReference>>()),Times.Never,"Content should not have triggered Create Variant Page Cache call");
            _testManager.Verify(call => call.CreateActiveTestCache(), Times.Once, "Content should have triggered Create Active Test call");
            _tdc.Verify(call => call.GetTestDataFromCookie(It.IsAny<string>()), Times.Once(),
                "Content should have triggered call to get cookie data");
            _tdc.Verify(call => call.SaveTestDataToCookie(It.IsAny<TestDataCookie>()), Times.Never(),
                "Content should not have triggered call to save cookie data");
            _tdc.Verify(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()), Times.Never(),
                "Content should not have triggered call to update cookie data");
       }

        [Fact]
        public void TestHandler_ContentUnderTest_and_SwapDisabled_FallsThroughProcess()
        {
            List<ContentReference> contentRef = new List<ContentReference>();

            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            contentRef.Add(content.ContentLink);

            List<IMarketingTest> testList = new List<IMarketingTest>()
            {
                new ABTest() {OriginalItemId = _associatedTestGuid}
            };

            TestDataCookie testData = new TestDataCookie();


            var x = GetUnitUnderTest(contentRef, true,testData);
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
        public void TestHandler_ContentUnderTest_and_SwapEnabled_NoData_SetsUpNewTestData_WithVariant_AndSwaps()
        {
            List<ContentReference> contentRef = new List<ContentReference>();


            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            content.ContentLink = new ContentReference(6,0);





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
           TestDataCookie testData = new TestDataCookie();

          
            




            var x = GetUnitUnderTest(contentRef, false, testData);
            _testManager.Setup(call => call.GetTestByItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);
           _testManager.Setup(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(),It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(testData);
            _tdc.Setup(call => call.HasTestData(It.IsAny<TestDataCookie>())).Returns(false);
            //_tdc.Setup(call => call.IsTestParticipant(It.IsAny<TestDataCookie>())).Returns(false);




            ContentEventArgs args = new ContentEventArgs(content);
            x.LoadedContent(new object(), args);

            _testManager.Verify(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>()), Times.Once, "Content should have triggered Create Variant Page Cache call");
            _testManager.Verify(call => call.CreateActiveTestCache(), Times.Once, "Content should have triggered Create Active Test call");
            _testManager.Verify(call => call.IncrementCount(It.IsAny<Guid>(),It.IsAny<Guid>(),It.IsAny<int>(),CountType.View), Times.Once, "Content should have triggered IncrementCount View call");

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
            List<ContentReference> contentRef = new List<ContentReference>();


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
                Id = _matchingVariantId,
                ItemVersion = 0,
                TestId = _activeTestGuid,
                ItemId = _associatedTestGuid
            };
            TestDataCookie testData = new TestDataCookie();







            var x = GetUnitUnderTest(contentRef, false, testData);
            _testManager.Setup(call => call.GetTestByItemId(_associatedTestGuid)).Returns(testList);
            _testManager.Setup(call => call.ReturnLandingPage(_activeTestGuid)).Returns(testVariant);
            _testManager.Setup(call => call.CreateActiveTestCache()).Returns(testList);
            _testManager.Setup(call => call.CreateVariantPageDataCache(It.IsAny<Guid>(), It.IsAny<List<ContentReference>>())).Returns(new PageData(content.ContentLink as PageReference));
            _tdc.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(testData);
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
            List<ContentReference> contentRef = new List<ContentReference>();
            contentRef.Add(new ContentReference());

            IContent content = new BasicContent();
            content.ContentGuid = _associatedTestGuid;
            contentRef.Add(content.ContentLink);

            List<IMarketingTest> testList = new List<IMarketingTest>()
            {
                new ABTest() {OriginalItemId = _associatedTestGuid}
            };

            TestDataCookie testData = new TestDataCookie();


            var x = GetUnitUnderTest(contentRef, true, testData);
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


    }


}
