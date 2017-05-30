using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using System;
using System.Net;
using EPiServer.Marketing.Testing.Core.DataClass;
using Xunit;
using EPiServer.Marketing.Testing.Web.Helpers;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ABTestStoreTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<IMarketingTestingWebRepository> _webRepo = new Mock<IMarketingTestingWebRepository>();
        Mock<IEpiserverHelper> _episerverHelper = new Mock<IEpiserverHelper>();

        Mock<ILogger> _logger = new Mock<ILogger>();

        private ABTestStore GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ILogger>()).Returns(_logger.Object);
            _locator.Setup(sl => sl.GetInstance<IMarketingTestingWebRepository>()).Returns(_webRepo.Object);
            _locator.Setup(sl => sl.GetInstance<IEpiserverHelper>()).Returns(_episerverHelper.Object);

            ABTestStore testStore = new ABTestStore(_locator.Object);
            return testStore;
        }

        [Fact]
        public void Get_Returns_Valid_Response()
        {
            var testClass = GetUnitUnderTest();
            Guid contentGuid = Guid.NewGuid();
            Guid testGuid = Guid.NewGuid();

            CultureInfo currentCulture = new CultureInfo("en-GB");

            ABTest test = new ABTest()
            {
                Id = testGuid,
                ContentLanguage = "en-GB"
            };

            _webRepo.Setup(m => m.GetActiveTestForContent(
               It.Is<Guid>(arg=>arg.Equals(contentGuid)), It.Is<CultureInfo>(arg=>arg.Equals(currentCulture)))).Returns(test);
            _episerverHelper.Setup(call => call.GetContentCultureinfo()).Returns(currentCulture);
            RestResult result = testClass.Get(contentGuid.ToString()) as RestResult;

            _webRepo.Verify(m => m.GetActiveTestForContent(It.Is<Guid>(arg => arg.Equals(contentGuid)), It.Is<CultureInfo>(arg => arg.Equals(currentCulture))),
                "Guid passed to web repo did not match what was passed in to ABTestStore");

            Assert.True(result.Data is IMarketingTest, "Data in result is not IMarketingTest instance.");
            Assert.True((result.Data as IMarketingTest).Id == test.Id, "IMarketingTest.Id name does not match");
        }

        [Fact]
        public void Get_Captures_Exception_Returns_Valid_Response()
        {
            var testClass = GetUnitUnderTest();
            Guid contentGuid = Guid.NewGuid();

            _webRepo.Setup(m => m.GetActiveTestForContent(
                It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Throws(new NotImplementedException());

            RestStatusCodeResult result = testClass.Get(contentGuid.ToString()) as RestStatusCodeResult;

            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating a failure.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.InternalServerError, "Expected internal server error status code but got something else.");
        }

        [Fact]
        public void Delete_Returns_Valid_Response()
        {
            var testClass = GetUnitUnderTest();
            Guid contentGuid = Guid.NewGuid();
            Guid testGuid = Guid.NewGuid();

            ABTest test = new ABTest()
            {
                Id = testGuid
            };

            _webRepo.Setup(m => m.DeleteTestForContent(It.Is<Guid>(arg => arg.Equals(contentGuid)))).Verifiable();

            RestStatusCodeResult result = testClass.Delete(contentGuid.ToString()) as RestStatusCodeResult;

            _webRepo.Verify(m => m.DeleteTestForContent(It.Is<Guid>(arg => arg.Equals(contentGuid))),
                "Guid passed to web repo did not match what was passed in to ABTestStore for Delete");

            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating success.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.OK, "Expected ok status code but got something else.");
        }

        [Fact]
        public void Delete_Captures_Exception_Returns_Valid_Response()
        {
            var testClass = GetUnitUnderTest();
            Guid contentGuid = Guid.NewGuid();

            _webRepo.Setup(m => m.DeleteTestForContent(It.IsAny<Guid>())).Throws(new NotImplementedException());

            RestStatusCodeResult result = testClass.Delete(contentGuid.ToString()) as RestStatusCodeResult;

            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating a failure.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.InternalServerError, "Expected internal server error status code but got something else.");
        }

        [Fact]
        public void Post_Returns_Valid_Response_WithValidCultureName()
        {
            var testClass = GetUnitUnderTest();

            TestingStoreModel testData = new TestingStoreModel();
            Guid returnGuid = Guid.NewGuid();

            _webRepo.Setup(m => m.CreateMarketingTest(It.Is<TestingStoreModel>(arg => arg.Equals(testData)))).Returns(returnGuid);
            _episerverHelper.Setup(m => m.GetContentCultureinfo()).Returns(new System.Globalization.CultureInfo("zh-CHT"));
            RestStatusCodeResult result = testClass.Post(testData) as RestStatusCodeResult;
            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating success.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.Created, "Expected Created status code but got something else.");
        }

        [Fact]
        public void Post_Returns_Valid_Response_WithUnspecifiedCulture()
        {
            var testClass = GetUnitUnderTest();

            TestingStoreModel testData = new TestingStoreModel();
            Guid returnGuid = Guid.NewGuid();

            _webRepo.Setup(m => m.CreateMarketingTest(It.Is<TestingStoreModel>(arg => arg.Equals(testData)))).Returns(returnGuid);
            _episerverHelper.Setup(m => m.GetContentCultureinfo()).Returns((CultureInfo)null);
            RestStatusCodeResult result = testClass.Post(testData) as RestStatusCodeResult;
            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating success.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.Created, "Expected Created status code but got something else.");
        }

        [Fact]
        public void Post_Returns_Valid_Response_Repo_Returns_Empty_Guid()
        {
            var testClass = GetUnitUnderTest();

            TestingStoreModel testData = new TestingStoreModel();

            _webRepo.Setup(m => m.CreateMarketingTest(It.Is<TestingStoreModel>(arg => arg.Equals(testData)))).Returns(Guid.Empty);

            RestStatusCodeResult result = testClass.Post(testData) as RestStatusCodeResult;

            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating failure.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.InternalServerError, "Expected InternalServerError status code but got something else.");
        }

        [Fact]
        public void Post_Returns_Valid_Response_Repo_Throws_Exception()
        {
            var testClass = GetUnitUnderTest();

            TestingStoreModel testData = new TestingStoreModel();

            _webRepo.Setup(m => m.CreateMarketingTest(It.Is<TestingStoreModel>(arg => arg.Equals(testData)))).Throws(new NotImplementedException());

            RestStatusCodeResult result = testClass.Post(testData) as RestStatusCodeResult;

            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating failure.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.InternalServerError, "Expected InternalServerError status code but got something else.");
        }

        [Fact]
        public void Put_Returns_Valid_Response()
        {
            var testClass = GetUnitUnderTest();

            TestResultStoreModel testData = new TestResultStoreModel();
            Guid returnGuid = Guid.NewGuid();

            _webRepo.Setup(m => m.PublishWinningVariant(It.Is<TestResultStoreModel>(arg => arg.Equals(testData)))).Returns(returnGuid.ToString());

            RestResult result = testClass.Put(testData) as RestResult;

            Assert.True(result != null, "Expected a RestResult indicating success.");
            Assert.True(result.Data as string == returnGuid.ToString(), "Expected put status value to be the return Guid.");
        }

        [Fact]
        public void Put_Returns_Valid_Response_Repo_Returns_Empty_Guid()
        {
            var testClass = GetUnitUnderTest();

            TestResultStoreModel testData = new TestResultStoreModel();

            _webRepo.Setup(m => m.PublishWinningVariant(It.Is<TestResultStoreModel>(arg => arg.Equals(testData)))).Returns("");

            RestStatusCodeResult result = testClass.Put(testData) as RestStatusCodeResult;

            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating failure.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.InternalServerError, "Expected InternalServerError status code but got something else.");
        }
        [Fact]
        public void Put_Returns_Valid_Response_Repo_Throws_Exception()
        {
            var testClass = GetUnitUnderTest();

            TestResultStoreModel testData = new TestResultStoreModel();

            _webRepo.Setup(m => m.PublishWinningVariant(It.Is<TestResultStoreModel>(arg => arg.Equals(testData)))).Throws(new NotImplementedException());

            RestStatusCodeResult result = testClass.Put(testData) as RestStatusCodeResult;

            // soft type cast will make result null if its not a RestStatusCodeResult
            Assert.True(result != null, "Expected a RestStatusCodeResult indicating failure.");
            // make sure return status code is correct.
            Assert.True(result.StatusCode == (int)HttpStatusCode.InternalServerError, "Expected InternalServerError status code but got something else.");
        }
    }
}
