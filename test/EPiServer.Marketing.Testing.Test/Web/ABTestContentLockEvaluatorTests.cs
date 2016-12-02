using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Evaluator;
using EPiServer.Marketing.Testing.Web.Repositories;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ABTestContentLockEvaluatorTests
    {
        private Mock<IMarketingTestingWebRepository> _mockWebRepo;
        private Mock<IContentRepository> _mockContentRepo;
        
        private ABTestLockEvaluator GetUnitUnderTest()
        {
            _mockWebRepo = new Mock<IMarketingTestingWebRepository>();
            _mockContentRepo = new Mock<IContentRepository>();

            return new ABTestLockEvaluator(_mockWebRepo.Object, _mockContentRepo.Object);
        }

        [Fact]
        public void IsLocked_Returns_A_Lock_When_The_Content_Has_An_Active_Test_Against_It()
        {
            var aAbLockEvaluator = GetUnitUnderTest();
            var testContent = new BasicContent() { ContentGuid = new Guid() };
            var mockTest = new ABTest() {
                Id = Guid.NewGuid(),
                Owner = "Some Guy",
                CreatedDate = DateTime.Now
            };
            _mockContentRepo.Setup(contentRepo => contentRepo.Get<IContent>(It.IsAny<ContentReference>())).Returns(testContent);
            _mockWebRepo.Setup(webRepo => webRepo.GetActiveTestForContent(It.IsAny<Guid>())).Returns(mockTest);

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
