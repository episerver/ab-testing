using System;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Helpers;
using Moq;
using Xunit;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Data.Enums;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestDataCookieHelperTest : IDisposable
    {
        private Mock<ITestManager> _testManager;

        private Guid _activeTestId = Guid.Parse("a194bde9-af3c-40fa-9635-338d02f5dea4");
        private Guid _inactiveTestId = Guid.Parse("5e2f21e3-30f7-4dcf-89cd-b9d7ff8c7cd6");
        private Guid _testContentGuid = Guid.Parse("6d532659-006d-453b-b7f4-62a94b2f3a3c");
        private Guid _testVariantGuid = Guid.Parse("9226a971-605c-4fd7-8a5c-aa7873e1e818");
        private DateTime _testEndDateTime = DateTime.Parse("5/5/2050");

        private IMarketingTest _activeTest;
        private IMarketingTest _inactiveTest;

        public TestDataCookieHelperTest()
        {
            HttpContext.Current = new HttpContext(
   new HttpRequest(null, "http://tempuri.org", null),
   new HttpResponse(null));
        }
        
        private TestDataCookieHelper GetUnitUnderTest()
        {
            _testManager = new Mock<ITestManager>();

            _activeTest = new ABTest()
            {
                Id = _activeTestId,
                EndDate = _testEndDateTime,
                State = TestState.Active,
                KpiInstances = new List<IKpi>()
             };

            _inactiveTest = new ABTest()
            {
                Id = _inactiveTestId,
                EndDate = _testEndDateTime,
                State = TestState.Archived,
                KpiInstances = new List<IKpi>()
            };

            _testManager.Setup(call => call.Get(_activeTestId)).Returns(_activeTest);
            _testManager.Setup(call => call.Get(_inactiveTestId)).Returns(_inactiveTest);


            return new TestDataCookieHelper(_testManager.Object);
        }

        [Fact]
        public void HasTestData_Returns_False_If_TestContentId_Is_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            TestDataCookie testData = new TestDataCookie();
            testData.TestContentId = Guid.Empty;
            bool hasTestData = testDataCookieHelper.HasTestData(testData);
            Assert.False(hasTestData);
        }

        [Fact]
        public void HasTestData_Returns_True_If_TestContentId_Is_Not_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            TestDataCookie testData = new TestDataCookie();
            testData.TestContentId = Guid.NewGuid();
            bool hasTestData = testDataCookieHelper.HasTestData(testData);
            Assert.True(hasTestData);
        }

        [Fact]
        public void HasTestData_Returns_False_If_TestVariantId_Is_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            TestDataCookie testData = new TestDataCookie();
            testData.TestVariantId = Guid.Empty;
            bool isTestParticipant = testDataCookieHelper.IsTestParticipant(testData);
            Assert.False(isTestParticipant);
        }

        [Fact]
        public void HasTestData_Returns_True_If_TestVariantId_Is_Not_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            TestDataCookie testData = new TestDataCookie();
            testData.TestVariantId = Guid.NewGuid();
            bool isTestParticipant = testDataCookieHelper.IsTestParticipant(testData);
            Assert.True(isTestParticipant);
        }

        [Fact]
        public void GetTestDataFromCookie_Returns_Correct_Values_For_Active_Test_From_Populated_Cookie()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var TestContentId = Guid.NewGuid();
            DateTime ExpireDate = DateTime.Now.AddDays(2);

            var ShowVariant = true;
            var TestVariantId = Guid.NewGuid();
            var Viewed = false;
            var Converted = false;
            
            HttpCookie testCookie = new HttpCookie("EPI-MAR-" + TestContentId.ToString())
            {
                ["TestId"] = _activeTestId.ToString(),
                ["ShowVariant"] = ShowVariant.ToString(),
                ["TestContentId"] = TestContentId.ToString(),
                ["TestVariantId"] = TestVariantId.ToString(),
                ["Viewed"] = Viewed.ToString(),
                ["Converted"] = Converted.ToString(),
                Expires = ExpireDate,
                [Guid.NewGuid().ToString() + "-Flag"] = true.ToString()
            };

            HttpContext.Current.Request.Cookies.Add(testCookie);

            TestDataCookie returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie(TestContentId.ToString());
            Assert.True(returnCookieData.TestId == _activeTestId);
            Assert.True(returnCookieData.TestContentId == TestContentId);
            Assert.True(returnCookieData.ShowVariant == ShowVariant);
            Assert.True(returnCookieData.TestVariantId == TestVariantId);
            Assert.True(returnCookieData.Viewed == Viewed);
            Assert.True(returnCookieData.Converted == Converted);
        }

        [Fact]
        public void GetTestDataFromCookie_Resets_Values_For_Inactive_Test()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var TestContentId = Guid.NewGuid();
            DateTime ExpireDate = DateTime.Now.AddDays(2);

            var ShowVariant = true;
            var TestVariantId = Guid.NewGuid();
            var Viewed = false;
            var Converted = false;

            HttpCookie testCookie = new HttpCookie("EPI-MAR-" + TestContentId.ToString())
            {
                ["TestId"] = _inactiveTestId.ToString(),
                ["ShowVariant"] = ShowVariant.ToString(),
                ["TestContentId"] = TestContentId.ToString(),
                ["TestVariantId"] = TestVariantId.ToString(),
                ["Viewed"] = Viewed.ToString(),
                ["Converted"] = Converted.ToString(),
                Expires = ExpireDate,
                [Guid.NewGuid().ToString() + "-Flag"] = true.ToString()
            };

            HttpContext.Current.Request.Cookies.Add(testCookie);

            TestDataCookie returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie(TestContentId.ToString());
            Assert.True(returnCookieData.TestId == Guid.Empty);
            Assert.True(returnCookieData.TestContentId == Guid.Empty);
            Assert.True(returnCookieData.ShowVariant == false);
            Assert.True(returnCookieData.TestVariantId == Guid.Empty);
            Assert.True(returnCookieData.Viewed == false);
            Assert.True(returnCookieData.Converted == false);
        }

        [Fact]
        public void GetTestDataFromCookie_Returns_EmptyData_When_CookieIsNotAvailable()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var invalidTesetContentId = Guid.NewGuid();

            TestDataCookie returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie("EPI-MAR-" + invalidTesetContentId.ToString());
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
                TestId = _activeTest.Id,
                TestContentId = _testContentGuid,
                TestVariantId = _testVariantGuid,
                ShowVariant = true,
                Viewed = true,
                Converted = false
            };

           
            
            mockTesteDataCookiehelper.SaveTestDataToCookie(tdCookie);
            var cookieValue = HttpContext.Current.Response.Cookies.Get("EPI-MAR-" + tdCookie.TestContentId.ToString());

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
                TestId = _activeTest.Id,
                TestContentId = _testContentGuid,
                TestVariantId = _testVariantGuid,
                ShowVariant = true,
                Viewed = true,
                Converted = false
            };

            TestDataCookie updatedCookie = new TestDataCookie()
            {
                TestId = _activeTest.Id,
                TestContentId = _testContentGuid,
                TestVariantId = _testVariantGuid,
                ShowVariant = false,
                Viewed = false,
                Converted = true

            };
            
            mockTesteDataCookiehelper.SaveTestDataToCookie(originalCookie);
            mockTesteDataCookiehelper.UpdateTestDataCookie(updatedCookie);



            HttpCookie cookieValue = HttpContext.Current.Response.Cookies.Get("EPI-MAR-" + updatedCookie.TestContentId.ToString());

            Assert.True(cookieValue != null);
            Assert.True(Guid.Parse(cookieValue["TestId"]) == updatedCookie.TestId);
            Assert.True(Guid.Parse(cookieValue["TestContentId"]) == updatedCookie.TestContentId);
            Assert.True(Guid.Parse(cookieValue["TestVariantId"]) == updatedCookie.TestVariantId);
            Assert.True(bool.Parse(cookieValue["ShowVariant"]) == updatedCookie.ShowVariant);
            Assert.True(bool.Parse(cookieValue["Viewed"]) == updatedCookie.Viewed);
            Assert.True(bool.Parse(cookieValue["Converted"]) == updatedCookie.Converted);



        }

        [Fact]
        public void ExpireTestCookieData_Properly_Expires_A_Cookie()
        {
            var testDataCookieHelper = GetUnitUnderTest();

            TestDataCookie tdCookie = new TestDataCookie()
            {
                TestId = _activeTest.Id,
            };

            testDataCookieHelper.SaveTestDataToCookie(tdCookie);
            var cookieValue = HttpContext.Current.Response.Cookies.Get("EPI-MAR-" + tdCookie.TestContentId.ToString());
            Assert.True(cookieValue != null);
            testDataCookieHelper.ExpireTestDataCookie(tdCookie);
            cookieValue = HttpContext.Current.Response.Cookies.Get("EPI-MAR-" + tdCookie.TestContentId.ToString());
            Assert.True(cookieValue != null && cookieValue.Expires <= DateTime.Now.AddDays(-1d));

        }


        public void Dispose()
        {
            HttpContext.Current = null;
        }
    }
}
