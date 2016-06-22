using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using EPiServer.Data.Dynamic;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ContentTestStoreTests
    {
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<IMarketingTestingWebRepository> _mockMarketingTestingWebRepository;

        public ContentTestStore GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();
            _mockMarketingTestingWebRepository = new Mock<IMarketingTestingWebRepository>();

            _mockServiceLocator.Setup(call => call.GetInstance<IMarketingTestingWebRepository>())
                .Returns(_mockMarketingTestingWebRepository.Object);

            return new ContentTestStore(_mockServiceLocator.Object);
        }

        [Fact]
        public void Get_Action_With_Valid_Id_Returns_Valid_Test_Data()
        {
            IMarketingTest testData = new ABTest()
            {
                Id=Guid.NewGuid(),
                Title = "Content Test Store Title",
                Variants = new List<Variant>() { new Variant() { Id=Guid.NewGuid()} }
            };

            var contentTestStore = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(call => call.GetActiveTestForContent(It.IsAny<Guid>()))
                .Returns(testData);

            var testResult = contentTestStore.Get(Guid.NewGuid().ToString());
            Assert.IsType<RestResult>(testResult);

            var resultData = (RestResult) testResult;
            var abTestData = (ABTest)resultData.Data;
            Assert.True(abTestData.Id == testData.Id);
            Assert.True(abTestData.Title == testData.Title);
            Assert.True(abTestData.Variants[0].Id == testData.Variants[0].Id);
        }

        [Fact]
        public void Delete_Action_Succeeds_With_Valid_Id()
        {
            var contentTestStore = GetUnitUnderTest();
            var testResult = contentTestStore.Delete(Guid.NewGuid().ToString());
            _mockMarketingTestingWebRepository.Setup(call => call.DeleteTestForContent(It.IsAny<Guid>()));

            Assert.IsType<RestStatusCodeResult>(testResult);
            var code = testResult.ToPropertyBag()["StatusCode"].ToString();
            Assert.True(code == "200");
        }

        [Fact]
        public void Delete_Action_Fails_With_InValid_Id()
        {
            var contentTestStore = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(call => call.DeleteTestForContent(It.IsAny<Guid>())).Throws(new ServerException());

            var testResult = contentTestStore.Delete("abc");

            Assert.IsType<RestStatusCodeResult>(testResult);
            var code = testResult.ToPropertyBag()["StatusCode"].ToString();
            Assert.True(code == "500");
        }
    }
}
