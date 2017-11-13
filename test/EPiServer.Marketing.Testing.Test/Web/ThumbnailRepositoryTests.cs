using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.IO;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ThumbnailRepositoryTests
    {
        Mock<IServiceLocator> _mockServiceLocator = new Mock<IServiceLocator>();
        Mock<IHttpContextHelper> _mockContextHelper = new Mock<IHttpContextHelper>();
        Mock<IProcessHelper> _mockProcessHelper = new Mock<IProcessHelper>();
        private ThumbnailRepository GetUnitUnderTest()
        {
            _mockServiceLocator.Setup(sl => sl.GetInstance<IHttpContextHelper>()).Returns(_mockContextHelper.Object);
            _mockServiceLocator.Setup(sl => sl.GetInstance<IProcessHelper>()).Returns(_mockProcessHelper.Object);
            return new ThumbnailRepository(_mockServiceLocator.Object);
        }

        [Fact]
        public void GetRandomFileName_ReturnsGuidBasedFileName()
        {
            var thumbRepo = GetUnitUnderTest();
            Guid guidOut;
            var fileName = thumbRepo.GetRandomFileName();
            Assert.True(Guid.TryParse(fileName.Split('.')[0], out guidOut));
            Assert.True(fileName.Split('.')[1] == "png");
        }

        [Fact]
        public void GetCaptureProcess_ReturnsInitializedProcess()
        {
            _mockProcessHelper.Setup(call => call.GetProcessRootPath()).Returns("ROOTPATH");
            _mockProcessHelper.Setup(call => call.GetThumbnailExecutablePath()).Returns("EXECUTABLE");

            var thumbRepo = GetUnitUnderTest();
            var proc = thumbRepo.GetCaptureProcess("ID", "FILENAME", new ContextThumbData() { host = "host", pagePrefix = "http://", authCookie = "APPCOOKIE", sessionCookie = "SESSONCOOKIE" });
            Assert.True(proc.StartInfo.FileName == "EXECUTABLE");
            Assert.True(proc.StartInfo.WorkingDirectory == "ROOTPATH");
            Assert.False(proc.StartInfo.UseShellExecute);
            Assert.True(proc.StartInfo.CreateNoWindow);
            Assert.True(proc.StartInfo.RedirectStandardOutput);
            Assert.True(proc.StartInfo.RedirectStandardInput);
        }

        [Fact]
        public void GetContextThumbData_ReturnsPopulatedContextThumbDatObject_whenAspAppCookieSet()
        {
            HttpRequest mockRequest = new HttpRequest("", "http://mock.url/path", "");
            HttpResponse mockResponse = new HttpResponse(new StringWriter());
            HttpCookie mockSessionCookie = new HttpCookie("ASP.NET_SessionId");
            HttpCookie mockApplicationCookie = new HttpCookie(".AspNet.ApplicationCookie");
            mockSessionCookie.Value = "MOCKSESSIONDATA";
            mockApplicationCookie.Value = "MOCKAPPLICATOINDATA";
            mockRequest.Cookies.Add(mockSessionCookie);
            mockRequest.Cookies.Add(mockApplicationCookie);

            HttpContext mockContext = new HttpContext(mockRequest, mockResponse);
            _mockContextHelper.Setup(call => call.GetCurrentContext()).Returns(mockContext);

            var thumbRepo = GetUnitUnderTest();
            ContextThumbData testData = thumbRepo.GetContextThumbData();
            Assert.True(testData.authCookie == mockApplicationCookie.Name+"|"+mockApplicationCookie.Value);
            Assert.True(testData.sessionCookie == mockSessionCookie.Value);
            Assert.True(testData.host == "mock.url");
            Assert.True(testData.pagePrefix == "http://mock.url");
        }

        [Fact]
        public void GetContextThumbData_ReturnsPopulatedContextThumbDatObject_whenEpiAppCookieSet()
        {
            HttpRequest mockRequest = new HttpRequest("", "http://mock.url/path", "");
            HttpResponse mockResponse = new HttpResponse(new StringWriter());
            HttpCookie mockSessionCookie = new HttpCookie("ASP.NET_SessionId");
            HttpCookie mockApplicationCookie = new HttpCookie(".EPiServerLogin");
            mockSessionCookie.Value = "MOCKSESSIONDATA";
            mockApplicationCookie.Value = "MOCKAPPLICATOINDATA";
            mockRequest.Cookies.Add(mockSessionCookie);
            mockRequest.Cookies.Add(mockApplicationCookie);

            HttpContext mockContext = new HttpContext(mockRequest, mockResponse);
            _mockContextHelper.Setup(call => call.GetCurrentContext()).Returns(mockContext);

            var thumbRepo = GetUnitUnderTest();
            ContextThumbData testData = thumbRepo.GetContextThumbData();
            Assert.True(testData.authCookie == mockApplicationCookie.Name + "|" + mockApplicationCookie.Value);
            Assert.True(testData.sessionCookie == mockSessionCookie.Value);
            Assert.True(testData.host == "mock.url");
            Assert.True(testData.pagePrefix == "http://mock.url");
        }

    }
} 
 