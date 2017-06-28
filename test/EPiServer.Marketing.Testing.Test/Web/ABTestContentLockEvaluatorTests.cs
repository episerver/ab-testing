using System;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Web.Evaluator;
using EPiServer.Marketing.Testing.Web.Repositories;
using Moq;
using Xunit;
using EPiServer.Marketing.Testing.Web.Helpers;
using System.Globalization;
using EPiServer.Cms.Shell;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.SpecializedProperties;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ABTestContentLockEvaluatorTests
    {
        private Mock<IMarketingTestingWebRepository> _mockWebRepo;
        private Mock<IContentRepository> _mockContentRepo;
        private Mock<IEpiserverHelper> _mockEpiHelper;
        
        private ABTestLockEvaluator GetUnitUnderTest()
        {
            _mockWebRepo = new Mock<IMarketingTestingWebRepository>();
            _mockContentRepo = new Mock<IContentRepository>();
            _mockEpiHelper = new Mock<IEpiserverHelper>();

            return new ABTestLockEvaluator(_mockWebRepo.Object, _mockContentRepo.Object, _mockEpiHelper.Object);
        }

        [Fact]
        public void IsLocked_Returns_A_Lock_When_The_Content_Has_An_Active_Test_Against_It()
        {
            var aAbLockEvaluator = GetUnitUnderTest();
            var testContent = new FakeLocalizableContent() {Language = new CultureInfo("en-GB"), MasterLanguage = new CultureInfo("en-GB")};
            //testContent.Property.Add(new PropertyLanguage() {Name = "en-GB"});
            //testContent.Property.LanguageBranch = "en-GB";

            var mockTest = new ABTest() {
                Id = Guid.NewGuid(),
                Owner = "Some Guy",
                CreatedDate = DateTime.Now,
                ContentLanguage = "en-GB"
            };

            _mockEpiHelper.Setup(epiHelper => epiHelper.GetContentCultureinfo()).Returns(new CultureInfo("en-GB"));
            _mockContentRepo.Setup(contentRepo => contentRepo.Get<IContent>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>())).Returns(testContent);
            _mockWebRepo.Setup(webRepo => webRepo.GetActiveTestForContent(It.IsAny<Guid>(),It.IsAny<CultureInfo>())).Returns(mockTest);
            
            var retLock = aAbLockEvaluator.IsLocked(new ContentReference() { ID = 1, WorkID = 1 });

            Assert.NotNull(retLock);
            Assert.Equal(aAbLockEvaluator._abLockId, retLock.LockIdentifier);
            Assert.Equal(mockTest.Owner, retLock.LockedBy);
        }

        [Fact]
        public void IsLocked_Does_Not_Lock_When_There_Is_No_Test()
        {
            var aAbLockEvaluator = GetUnitUnderTest();
            var testContent = new BasicContent() { ContentGuid = new Guid() };
            var mockTest = new ABTest();

            _mockContentRepo.Setup(contentRepo => contentRepo.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockWebRepo.Setup(webRepo => webRepo.GetActiveTestForContent(It.IsAny<Guid>())).Returns(mockTest);

            var retLock = aAbLockEvaluator.IsLocked(new ContentReference() { ID = 1, WorkID = 1 });

            Assert.Null(retLock);
        }

        [Fact]
        public void IsLocked_Does_Not_Lock_When_It_Can_Not_Find_The_Content_Passed_In()
        {
            var aAbLockEvaluator = GetUnitUnderTest();
            BasicContent testContent = null;

            _mockContentRepo.Setup(contentRepo => contentRepo.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);

            var retLock = aAbLockEvaluator.IsLocked(new ContentReference() { ID = 1, WorkID = 1 });

            Assert.Null(retLock);
        }
    }
}
