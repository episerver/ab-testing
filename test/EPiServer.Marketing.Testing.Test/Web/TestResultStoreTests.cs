using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestResultStoreTests
    {
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<IContentRepository> _mockContentRespository;
        private Mock<ITestResultHelper> _mockTestResultHelper;
        private Mock<IContentVersionRepository> _mockContentVersionRepository;
        private Mock<IMarketingTestingWebRepository> _mockMarketingTestingWebRepository;
        private Mock<IUIHelper> _mockUiHelper;

        private TestResultStore GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();
            _mockContentRespository = new Mock<IContentRepository>();
            _mockTestResultHelper = new Mock<ITestResultHelper>();
            _mockContentVersionRepository = new Mock<IContentVersionRepository>();
            _mockMarketingTestingWebRepository = new Mock<IMarketingTestingWebRepository>();
            _mockUiHelper = new Mock<IUIHelper>();
            
            _mockServiceLocator.Setup(call => call.GetInstance<IContentRepository>())
                .Returns(_mockContentRespository.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IContentVersionRepository>())
                .Returns(_mockContentVersionRepository.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IMarketingTestingWebRepository>())
                .Returns(_mockMarketingTestingWebRepository.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<ITestResultHelper>())
                .Returns(_mockTestResultHelper.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IUIHelper>()).Returns(_mockUiHelper.Object);

            return new TestResultStore(_mockServiceLocator.Object);
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

            TestResultStore trs = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(It.IsAny<Guid>())).Returns(test);
            _mockMarketingTestingWebRepository.Setup(
                call => call.ArchiveMarketingTest(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()));
            _mockMarketingTestingWebRepository.Setup(
                call => call.ArchiveMarketingTest(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()));

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                            reference => reference == ContentReference.Parse(testResultmodel.DraftContentLink))))
                .Returns(draftContent);

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                           reference => reference == ContentReference.Parse(testResultmodel.PublishedContentLink))))
               .Returns(publishedContent);

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == draftContent)))
                .Returns(new ContentReference{ID=10,WorkID = 101});

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == publishedContent)))
               .Returns(new ContentReference{ID=10,WorkID = 0});

            _mockUiHelper.Setup(call => call.getEpiUrlFromLink(It.IsAny<ContentReference>())).Returns("http://");
            
            ActionResult aResult = trs.Post(testResultmodel);
            var restResult = aResult as RestResult;
            Assert.True(restResult.Data.ToString() == testResultmodel.DraftContentLink.Split('_')[0]);
            _mockTestResultHelper.Verify(call => call.PublishContent(draftContent), Times.Once);
            _mockTestResultHelper.Verify(call => call.PublishContent(publishedContent), Times.Never);
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
                    new Variant() {ItemVersion = 10,Id=Guid.Parse("2ecb7bd5-33dd-44a5-aa05-8f82077b4896")}
                }

            };

            IContent publishedContent = new BasicContent();
            IContent draftContent = new BasicContent();

            TestResultStore trs = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(It.IsAny<Guid>())).Returns(test);
            _mockMarketingTestingWebRepository.Setup(
                call => call.ArchiveMarketingTest(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()));
            _mockMarketingTestingWebRepository.Setup(
                call => call.ArchiveMarketingTest(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()));

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

            _mockUiHelper.Setup(call => call.getEpiUrlFromLink(It.IsAny<ContentReference>())).Returns("http://");
            
            ActionResult aResult = trs.Post(testResultmodel);
            var restResult = aResult as RestResult;
            Assert.True(restResult.Data.ToString() == testResultmodel.DraftContentLink.Split('_')[0]);
            _mockTestResultHelper.Verify(call=>call.PublishContent(draftContent),Times.Once);
            _mockTestResultHelper.Verify(call => call.PublishContent(publishedContent), Times.Once);
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

            TestResultStore trs = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(call => call.GetTestById(It.IsAny<Guid>())).Returns((IMarketingTest)null);

            _mockTestResultHelper.Setup(call=>call.GetClonedContentFromReference(It.Is<ContentReference>(
                            reference => reference == ContentReference.Parse(testResultmodel.DraftContentLink))))
                .Returns(draftContent);

            _mockTestResultHelper.Setup(call => call.GetClonedContentFromReference(It.Is<ContentReference>(
                           reference => reference == ContentReference.Parse(testResultmodel.PublishedContentLink))))
               .Returns(publishedContent);

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == draftContent)))
                .Returns(new ContentReference { ID = 10, WorkID = 101 });

            _mockTestResultHelper.Setup(call => call.PublishContent(It.Is<IContent>(con => con == publishedContent)))
               .Returns(new ContentReference { ID = 10, WorkID = 0 });

            _mockUiHelper.Setup(call => call.getEpiUrlFromLink(It.IsAny<ContentReference>())).Returns("http://");

            ActionResult aResult = trs.Post(testResultmodel);
            RestStatusCodeResult restResult = aResult as RestStatusCodeResult;
            Assert.True(restResult != null && restResult.StatusCode == 500);
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

            TestResultStore trs = GetUnitUnderTest();
            ActionResult aResult = trs.Post(testResultModel);
            RestStatusCodeResult restResult = aResult as RestStatusCodeResult;
            Assert.True(restResult != null && restResult.StatusCode == 500);
        }
    }
}
