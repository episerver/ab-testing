using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ThumbnailStoreTests
    {
        Mock<IServiceLocator> _mockServiceLocator = new Mock<IServiceLocator>();
        Mock<IThumbnailRepository> _mockThumbRepo = new Mock<IThumbnailRepository>();
        Mock<IProcessHelper> _mockProcessHelper = new Mock<IProcessHelper>();

        private ThumbnailStore GetUnitUnderTest()
        {
            _mockServiceLocator.Setup(sl => sl.GetInstance<IThumbnailRepository>()).Returns(_mockThumbRepo.Object);
            return new ThumbnailStore(_mockServiceLocator.Object);

        }

        [Fact]
        public void Get_ReturnsRestString()
        {
            _mockThumbRepo.Setup(call => call.GetCaptureString(It.IsAny<string>())).Returns("returnstring");

            var tStore = GetUnitUnderTest();

            RestResult x = (RestResult)tStore.Get("testString");
            Assert.True(x.Data.ToString() == "returnstring");
        }

        [Fact]
        public void Get_RetunsError_WhenResultIsNull()
        {
            _mockThumbRepo.Setup(call => call.GetCaptureString(It.IsAny<string>())).Returns(string.Empty);
            var tStore = GetUnitUnderTest();

            RestStatusCodeResult x = tStore.Get("testString") as RestStatusCodeResult;
            Assert.True(x.StatusDescription.ToString() == "Error retrieving content thumbnail");
            Assert.True(x.StatusCode == 500);
        }
    }
}
