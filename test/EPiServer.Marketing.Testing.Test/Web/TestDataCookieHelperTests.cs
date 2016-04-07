using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestDataCookieHelperTest : IDisposable
    {
        private Mock<IServiceLocator> _serviceLocator;


        public TestDataCookieHelperTest()
        {
            HttpContext.Current = new HttpContext(
   new HttpRequest(null, "http://tempuri.org", null),
   new HttpResponse(null));
        }



        private TestDataCookieHelper GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            return new TestDataCookieHelper(_serviceLocator.Object);
        }

        [Fact]
        public void HasTestData_Returns_False_If_TestContentId_Is_Empty()
        {
            var mockTestDataCookieHelper = GetUnitUnderTest();
            TestDataCookie testData = new TestDataCookie();
            testData.TestContentId = Guid.Empty;
            bool hasTestData = mockTestDataCookieHelper.HasTestData(testData);
            Assert.False(hasTestData);
        }

        [Fact]
        public void HasTestData_Returns_True_If_TestContentId_Is_Not_Empty()
        {
            var mockTestDataCookieHelper = GetUnitUnderTest();
            TestDataCookie testData = new TestDataCookie();
            testData.TestContentId = Guid.NewGuid();
            bool hasTestData = mockTestDataCookieHelper.HasTestData(testData);
            Assert.True(hasTestData);
        }

        [Fact]
        public void HasTestData_Returns_False_If_TestVariantId_Is_Empty()
        {
            var mockTestDataCookieHelper = GetUnitUnderTest();
            TestDataCookie testData = new TestDataCookie();
            testData.TestVariantId = Guid.Empty;
            bool isTestParticipant = mockTestDataCookieHelper.IsTestParticipant(testData);
            Assert.False(isTestParticipant);
        }

        [Fact]
        public void HasTestData_Returns_True_If_TestVariantId_Is_Not_Empty()
        {
            var mockTestDataCookieHelper = GetUnitUnderTest();
            TestDataCookie testData = new TestDataCookie();
            testData.TestVariantId = Guid.NewGuid();
            bool isTestParticipant = mockTestDataCookieHelper.IsTestParticipant(testData);
            Assert.True(isTestParticipant);
        }

        [Fact]
        public void GetTestDataFromCookie_Returns_Correct_Values_From_Populated_Cookie()
        {


            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var TestContentId = Guid.NewGuid();
            DateTime ExpireDate = DateTime.Now.AddDays(2);

            var TestId = Guid.NewGuid();
            var ShowVariant = true;
            var TestParticipant = true;
            var TestVariantId = Guid.NewGuid();
            var Viewed = false;
            var Converted = false;



            HttpCookie testCookie = new HttpCookie(TestContentId.ToString())
            {
                ["TestId"] = TestId.ToString(),
                ["ShowVariant"] = ShowVariant.ToString(),
                ["TestContentId"] = TestContentId.ToString(),
                ["TestParticipant"] = TestParticipant.ToString(),
                ["TestVariantId"] = TestVariantId.ToString(),
                ["Viewed"] = Viewed.ToString(),
                ["Converted"] = Converted.ToString(),
                Expires = ExpireDate
            };

            HttpContext.Current.Request.Cookies.Add(testCookie);

            TestDataCookie returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie(TestContentId.ToString());
            Assert.True(returnCookieData.TestId == TestId);
            Assert.True(returnCookieData.TestContentId == TestContentId);
            Assert.True(returnCookieData.ShowVariant == ShowVariant);
            Assert.True(returnCookieData.TestParticipant == TestParticipant);
            Assert.True(returnCookieData.TestVariantId == TestVariantId);
            Assert.True(returnCookieData.Viewed == Viewed);
            Assert.True(returnCookieData.Converted == Converted);


        }

        [Fact]
        public void GetTestDataFromCookie_Returns_EmptyData_When_CookieIsNotAvailable()
        {


            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var TestContentId = Guid.NewGuid();
            var InvalidTesetContentId = Guid.NewGuid();
            DateTime ExpireDate = DateTime.Now.AddDays(2);

            var TestId = Guid.NewGuid();
            var ShowVariant = true;
            var TestParticipant = true;
            var TestVariantId = Guid.NewGuid();
            var Viewed = false;
            var Converted = false;



            HttpCookie testCookie = new HttpCookie(TestContentId.ToString())
            {
                ["TestId"] = TestId.ToString(),
                ["ShowVariant"] = ShowVariant.ToString(),
                ["TestContentId"] = TestContentId.ToString(),
                ["TestParticipant"] = TestParticipant.ToString(),
                ["TestVariantId"] = TestVariantId.ToString(),
                ["Viewed"] = Viewed.ToString(),
                ["Converted"] = Converted.ToString(),
                Expires = ExpireDate
            };

            HttpContext.Current.Request.Cookies.Add(testCookie);

            TestDataCookie returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie(InvalidTesetContentId.ToString());
            Assert.True(returnCookieData.TestId == Guid.Empty);
            Assert.True(returnCookieData.TestContentId == Guid.Empty);
            Assert.True(returnCookieData.ShowVariant == false);
            Assert.True(returnCookieData.TestParticipant == false);
            Assert.True(returnCookieData.TestVariantId == Guid.Empty);
            Assert.True(returnCookieData.Viewed == false);
            Assert.True(returnCookieData.Converted == false);


        }


        public void Dispose()
        {
            HttpContext.Current = null;
        }
    }
}
