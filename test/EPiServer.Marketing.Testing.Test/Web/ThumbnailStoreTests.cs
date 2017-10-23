using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
            _mockServiceLocator.Setup(sl => sl.GetInstance<IProcessHelper>()).Returns(_mockProcessHelper.Object);
            return new ThumbnailStore(_mockServiceLocator.Object);

        }

        [Fact]
        public void GetCallsPhantomAndReturnsFileName()
        {
            _mockThumbRepo.Setup(call => call.GetRandomFileName()).Returns("RandomFileName");
            _mockThumbRepo.Setup(call => call.GetContextThumbData()).Returns(new ContextThumbData()
            {
                applicationCookie = "appCookie",
                sessionCookie = "sessionCookie",
                host = "TestHost",
                pagePrefix = "Http://testhost.com"
            });

            var tStore = GetUnitUnderTest();

            RestResult x = (RestResult)tStore.Get("testId");
            _mockProcessHelper.Verify(call => call.startProcess(It.IsAny<Process>()), Times.Once, "Expected process helper startProcess to be called at least once");
            Assert.Equal(x.Data, "RandomFileName");
        }

        [Fact]
        public void DeleteCallsThumbRepoDeleteCaptureFile()
        {
            var tStore = GetUnitUnderTest();
            tStore.Delete("fakeId");
            _mockThumbRepo.Verify(call => call.DeleteCaptureFile(It.Is<string>(str => str == "fakeId")), Times.Once, "Expected Delete Caputure File to be called");
        }


    }
}
