using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestDataCookieHelperTest : IDisposable
    {
        private Mock<ITestManager> _testManager;

        private Guid _testId = Guid.Parse("a194bde9-af3c-40fa-9635-338d02f5dea4");
        private Guid _testContentGuid = Guid.Parse("6d532659-006d-453b-b7f4-62a94b2f3a3c");
        private Guid _testVariantGuid = Guid.Parse("9226a971-605c-4fd7-8a5c-aa7873e1e818");
        private DateTime _testEndDateTime = DateTime.Parse("5/5/2050");

        private IMarketingTest test;


        public TestDataCookieHelperTest()
        {
            HttpContext.Current = new HttpContext(
   new HttpRequest(null, "http://tempuri.org", null),
   new HttpResponse(null));
        }



        private TestDataCookieHelper GetUnitUnderTest()
        {
            _testManager = new Mock<ITestManager>();

            test = new ABTest()
             {
                 Id = _testId,
                 EndDate = _testEndDateTime
             };

            _testManager.Setup(call => call.Get(_testId)).Returns(test);

            return new TestDataCookieHelper(_testManager.Object);
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
            var TestVariantId = Guid.NewGuid();
            var Viewed = false;
            var Converted = false;



            HttpCookie testCookie = new HttpCookie(TestContentId.ToString())
            {
                ["TestId"] = TestId.ToString(),
                ["ShowVariant"] = ShowVariant.ToString(),
                ["TestContentId"] = TestContentId.ToString(),
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
            var TestVariantId = Guid.NewGuid();
            var Viewed = false;
            var Converted = false;



            HttpCookie testCookie = new HttpCookie(TestContentId.ToString())
            {
                ["TestId"] = TestId.ToString(),
                ["ShowVariant"] = ShowVariant.ToString(),
                ["TestContentId"] = TestContentId.ToString(),
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
            Assert.True(returnCookieData.TestVariantId == Guid.Empty);
            Assert.True(returnCookieData.Viewed == false);
            Assert.True(returnCookieData.Converted == false);


        }

        [Fact]
        public void SaveTestDataToCookie_ProperlySavesToContext()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();

            

            TestDataCookie tdCookie = new TestDataCookie()
            {
                TestId = test.Id,
                TestContentId = _testContentGuid,
                TestVariantId = _testVariantGuid,
                ShowVariant = true,
                Viewed = true,
                Converted = false
            };

           
            
            mockTesteDataCookiehelper.SaveTestDataToCookie(tdCookie);
            HttpCookie cookieValue = HttpContext.Current.Response.Cookies.Get(tdCookie.TestContentId.ToString());

            Assert.True(cookieValue != null);
            Assert.True(Guid.Parse(cookieValue["TestId"]) == tdCookie.TestId);
            Assert.True(Guid.Parse(cookieValue["TestContentId"]) == tdCookie.TestContentId);
            Assert.True(Guid.Parse(cookieValue["TestVariantId"]) == tdCookie.TestVariantId);
            Assert.True(bool.Parse(cookieValue["ShowVariant"]) == tdCookie.ShowVariant);
            Assert.True(bool.Parse(cookieValue["Viewed"]) == tdCookie.Viewed);
            Assert.True(bool.Parse(cookieValue["Converted"]) == tdCookie.Converted);
            


        }


        [Fact]
        public void UpdateTestDataCookie_ProperlyUpdatesCookie()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            


            TestDataCookie originalCookie = new TestDataCookie()
            {
                TestId = test.Id,
                TestContentId = _testContentGuid,
                TestVariantId = _testVariantGuid,
                ShowVariant = true,
                Viewed = true,
                Converted = false
            };

            TestDataCookie updatedCookie = new TestDataCookie()
            {
                TestId = test.Id,
                TestContentId = _testContentGuid,
                TestVariantId = _testVariantGuid,
                ShowVariant = false,
                Viewed = false,
                Converted = true

            };
            
            mockTesteDataCookiehelper.SaveTestDataToCookie(originalCookie);
            mockTesteDataCookiehelper.UpdateTestDataCookie(updatedCookie);



            HttpCookie cookieValue = HttpContext.Current.Response.Cookies.Get(updatedCookie.TestContentId.ToString());

            Assert.True(cookieValue != null);
            Assert.True(Guid.Parse(cookieValue["TestId"]) == updatedCookie.TestId);
            Assert.True(Guid.Parse(cookieValue["TestContentId"]) == updatedCookie.TestContentId);
            Assert.True(Guid.Parse(cookieValue["TestVariantId"]) == updatedCookie.TestVariantId);
            Assert.True(bool.Parse(cookieValue["ShowVariant"]) == updatedCookie.ShowVariant);
            Assert.True(bool.Parse(cookieValue["Viewed"]) == updatedCookie.Viewed);
            Assert.True(bool.Parse(cookieValue["Converted"]) == updatedCookie.Converted);



        }


        public void Dispose()
        {
            HttpContext.Current = null;
        }
    }
}
