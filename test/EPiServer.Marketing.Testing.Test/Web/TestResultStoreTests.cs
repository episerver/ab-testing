using EPiServer.Marketing.Testing.Web.Controllers;
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
        private Mock<IMarketingTestingWebRepository> _mockMarketingTestingWebRepository;

        private TestResultStore GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();
            _mockMarketingTestingWebRepository = new Mock<IMarketingTestingWebRepository>();
            _mockServiceLocator.Setup(call => call.GetInstance<IMarketingTestingWebRepository>())
                .Returns(_mockMarketingTestingWebRepository.Object);
           
            return new TestResultStore(_mockServiceLocator.Object);
        }

        [Fact]
        public void publishing_failure_returns_internal_server_error_code()
        {
            var resultStore = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(
                call => call.PublishWinningVariant(It.IsAny<TestResultStoreModel>())).Returns(-1);

            var aResult = resultStore.Post(new TestResultStoreModel());
            Assert.IsType<RestStatusCodeResult>(aResult);
            RestStatusCodeResult code = (RestStatusCodeResult) aResult;
           }

        [Fact]
        public void publishing_success_returns_rest_result_containing_integer()
        {
            var resultStore = GetUnitUnderTest();
            _mockMarketingTestingWebRepository.Setup(
                call => call.PublishWinningVariant(It.IsAny<TestResultStoreModel>())).Returns(20);

            var aResult = resultStore.Post(new TestResultStoreModel());
            Assert.IsType<RestResult>(aResult);
            RestResult restResult = (RestResult)aResult;
            Assert.True(restResult.Data.ToString() == "20");
        }
    }
}
