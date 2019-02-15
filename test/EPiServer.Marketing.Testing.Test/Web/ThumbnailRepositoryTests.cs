using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ThumbnailRepositoryTests
    {
        private Mock<IServiceLocator> _mockServiceLocator = new Mock<IServiceLocator>();
        private Mock<IHttpContextHelper> _mockContextHelper = new Mock<IHttpContextHelper>();
        private Mock<IProcessHelper> _mockProcessHelper = new Mock<IProcessHelper>();
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
            var proc = thumbRepo.GetCaptureProcess("ID", "FILENAME", new ContextThumbData() { host = "host", pagePrefix = "http://", cookieString = "COOKIE1 COOKIE2 COOKIE3" });
            Assert.True(proc.StartInfo.FileName == "EXECUTABLE");
            Assert.True(proc.StartInfo.WorkingDirectory == "ROOTPATH");
            Assert.False(proc.StartInfo.UseShellExecute);
            Assert.True(proc.StartInfo.CreateNoWindow);
            Assert.True(proc.StartInfo.RedirectStandardOutput);
            Assert.True(proc.StartInfo.RedirectStandardInput);
        }

        [Fact]
        public void GetContextThumbData_ReturnsPopulatedContextThumbDatObject_withCookieData()
        {
            HttpRequest mockRequest = new HttpRequest("", "http://mock.url/path", "");
            HttpResponse mockResponse = new HttpResponse(new StringWriter());
            HttpCookie mockCookie1 = new HttpCookie("MockCookie1");
            HttpCookie mockCookie2 = new HttpCookie("Mock.Cookie.2");
            HttpCookie mockCookie3 = new HttpCookie("_Mock.Cookie3");
            mockCookie1.Value = "Mockcookievalue1";
            mockCookie2.Value = "Mockcookievalue2";
            mockCookie3.Value = "Mockcookievalue3";
            List<string> mockCurrentCookieCollection = new List<string>();
            mockCurrentCookieCollection.Add(mockCookie1.Name + '*' + mockCookie1.Value);
            mockCurrentCookieCollection.Add(mockCookie2.Name + '*' + mockCookie2.Value);
            mockCurrentCookieCollection.Add(mockCookie3.Name + '*' + mockCookie3.Value);
           
            HttpContext mockContext = new HttpContext(mockRequest, mockResponse);
            _mockContextHelper.Setup(call => call.GetCurrentContext()).Returns(mockContext);
            _mockContextHelper.Setup(call => call.GetCurrentCookieCollection()).Returns(mockCurrentCookieCollection);

            var thumbRepo = GetUnitUnderTest();
            ContextThumbData testData = thumbRepo.GetContextThumbData();
            Assert.True(testData.host == "mock.url");
            Assert.True(testData.pagePrefix == "http://mock.url");
            Assert.True(testData.cookieString == $" {mockCookie1.Name}*{mockCookie1.Value} {mockCookie2.Name}*{mockCookie2.Value} {mockCookie3.Name}*{mockCookie3.Value}");
        }

        [Fact]
        public void GetContextThumbData_ReturnsPopulatedContextThumbDatObject_withEmptyCookieStringIfCookieCollectionIsNull()
        {
            HttpRequest mockRequest = new HttpRequest("", "http://mock.url/path", "");
            HttpResponse mockResponse = new HttpResponse(new StringWriter());

            HttpContext mockContext = new HttpContext(mockRequest, mockResponse);
            _mockContextHelper.Setup(call => call.GetCurrentContext()).Returns(mockContext);

            var thumbRepo = GetUnitUnderTest();
            ContextThumbData testData = thumbRepo.GetContextThumbData();
            Assert.True(testData.host == "mock.url");
            Assert.True(testData.pagePrefix == "http://mock.url");
            Assert.True(testData.cookieString == string.Empty);
        }




    }
}
