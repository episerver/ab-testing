using System;
using EPiServer.Data.Dynamic;
using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestingStoreTests
    {
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<IMarketingTestingWebRepository> _mockMarketingTestingWebRepository;

        public TestingStore GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();
            _mockMarketingTestingWebRepository = new Mock<IMarketingTestingWebRepository>();

            _mockServiceLocator.Setup(call => call.GetInstance<IMarketingTestingWebRepository>())
                .Returns(_mockMarketingTestingWebRepository.Object);

            return new TestingStore(_mockServiceLocator.Object);
        }

        [Fact]
        public void Successful_Test_Creation_Returns_Status_Created()
        {
            var testingStore = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(call => call.CreateMarketingTest(It.IsAny<TestingStoreModel>()))
                .Returns(Guid.NewGuid);

            var aResult = testingStore.Post(new TestingStoreModel());
            Assert.IsType<RestStatusCodeResult>(aResult);
            var  code = aResult.ToPropertyBag()["StatusCode"].ToString();
            Assert.True(code == "201");
        }

        [Fact]
        public void Failed_Test_Createion_Returns_Internal_Server_Error()
        {
            var testingStore = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(call => call.CreateMarketingTest(It.IsAny<TestingStoreModel>()))
                .Returns(null);

            var aResult = testingStore.Post(new TestingStoreModel());
            Assert.IsType<RestStatusCodeResult>(aResult);
            var code = aResult.ToPropertyBag()["StatusCode"].ToString();
            Assert.True(code == "500");
        }
    }
}
