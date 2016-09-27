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
            _testManager.Setup(call => call.GetActiveTestsByOriginalItemId(It.IsAny<Guid>())).Returns( 
                new List<IMarketingTest>() { _activeTest });

            return new TestDataCookieHelper(_testManager.Object);
        }

        [Fact]
        public void HasTestData_Returns_False_If_TestContentId_Is_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            var testData = new TestDataCookie {TestContentId = Guid.Empty};
            var hasTestData = testDataCookieHelper.HasTestData(testData);
            Assert.False(hasTestData);
        }

        [Fact]
        public void HasTestData_Returns_True_If_TestContentId_Is_Not_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            var testData = new TestDataCookie {TestContentId = Guid.NewGuid()};
            var hasTestData = testDataCookieHelper.HasTestData(testData);
            Assert.True(hasTestData);
        }

        [Fact]
        public void HasTestData_Returns_False_If_TestVariantId_Is_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            var testData = new TestDataCookie {TestVariantId = Guid.Empty};
            var isTestParticipant = testDataCookieHelper.IsTestParticipant(testData);
            Assert.False(isTestParticipant);
        }

        [Fact]
        public void HasTestData_Returns_True_If_TestVariantId_Is_Not_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            var testData = new TestDataCookie {TestVariantId = Guid.NewGuid()};
            var isTestParticipant = testDataCookieHelper.IsTestParticipant(testData);
            Assert.True(isTestParticipant);
        }

        [Fact]
        public void GetTestDataFromCookie_Returns_Correct_Values_For_Active_Test_From_Populated_Response_Cookie()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var expireDate = DateTime.Now.AddDays(2);

            var testVariantId = Guid.NewGuid();

            var testCookie = new HttpCookie("EPI-MAR-" + testContentId.ToString())
            {
                ["TestId"] = _activeTestId.ToString(),
                ["ShowVariant"] = "true",
                ["TestContentId"] = testContentId.ToString(),
                ["TestVariantId"] = testVariantId.ToString(),
                ["Viewed"] = "false",
                ["Converted"] = "false",
                Expires = expireDate,
                [Guid.NewGuid().ToString() + "-Flag"] = true.ToString()
            };

            HttpContext.Current.Response.Cookies.Add(testCookie);

            var returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie(testContentId.ToString());
            Assert.True(returnCookieData.TestId == _activeTestId);
            Assert.True(returnCookieData.TestContentId == testContentId);
            Assert.True(returnCookieData.ShowVariant);
            Assert.True(returnCookieData.TestVariantId == testVariantId);
            Assert.False(returnCookieData.Viewed);
            Assert.False(returnCookieData.Converted);
        }

        [Fact]
        public void GetTestDataFromCookie_Returns_Correct_Values_For_Active_Test_From_Populated_Request_Cookies()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var expireDate = DateTime.Now.AddDays(2);

            var testVariantId = Guid.NewGuid();

            var testCookie = new HttpCookie("EPI-MAR-" + testContentId.ToString())
            {
                ["TestId"] = _activeTestId.ToString(),
                ["ShowVariant"] = "true",
                ["TestContentId"] = testContentId.ToString(),
                ["TestVariantId"] = testVariantId.ToString(),
                ["Viewed"] = "false",
                ["Converted"] = "false",
                Expires = expireDate,
                [Guid.NewGuid().ToString() + "-Flag"] = true.ToString()
            };

            HttpContext.Current.Request.Cookies.Add(testCookie);

            var returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie(testContentId.ToString());
            Assert.True(returnCookieData.TestId == _activeTestId);
            Assert.True(returnCookieData.TestContentId == testContentId);
            Assert.True(returnCookieData.ShowVariant);
            Assert.True(returnCookieData.TestVariantId == testVariantId);
            Assert.False(returnCookieData.Viewed);
            Assert.False(returnCookieData.Converted);
        }

        [Fact]
        public void GetTestDataFromCookies_Returns_Correct_Values_For_Active_Test_From_Populated_Response_and_Request_Cookies()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var responseContentCookie1 = Guid.Parse("997636c1-fec2-43ff-8dd8-fa7f6b3ffa91");
            var requestContentCookie1 = Guid.Parse("997636c1-fec2-43ff-8dd8-fa7f6b3ffa91");
            var requestContentCookie2 = Guid.Parse("f222c926-999b-43f9-80dc-667f05e08260");

            var expireDate = DateTime.Now.AddDays(2);

            var responseCookie1 = new HttpCookie("EPI-MAR-" + responseContentCookie1.ToString())
            {
                ["TestId"] = _activeTestId.ToString(),
                ["ShowVariant"] = "True",
                ["TestContentId"] = responseContentCookie1.ToString(),
                ["TestVariantId"] = Guid.NewGuid().ToString(),
                ["Viewed"] = "True",
                ["Converted"] = "True",
                Expires = expireDate,
                [Guid.NewGuid().ToString() + "-Flag"] = true.ToString()
            };

            var requestCookie1 = new HttpCookie("EPI-MAR-" + requestContentCookie1.ToString())
            {
                ["TestId"] = _activeTestId.ToString(),
                ["ShowVariant"] = "True",
                ["TestContentId"] = requestContentCookie1.ToString(),
                ["TestVariantId"] = Guid.NewGuid().ToString(),
                ["Viewed"] = "True",
                ["Converted"] = "False",
                Expires = expireDate,
                [Guid.NewGuid().ToString() + "-Flag"] = true.ToString()
            };

            var requestCookie2 = new HttpCookie("EPI-MAR-" + requestContentCookie2.ToString())
            {
                ["TestId"] = _activeTestId.ToString(),
                ["ShowVariant"] = "True",
                ["TestContentId"] = requestContentCookie2.ToString(),
                ["TestVariantId"] = Guid.NewGuid().ToString(),
                ["Viewed"] = "True",
                ["Converted"] = "False",
                Expires = expireDate,
                [Guid.NewGuid().ToString() + "-Flag"] = true.ToString()
            };

            HttpContext.Current.Response.Cookies.Add(responseCookie1);
            HttpContext.Current.Request.Cookies.Add(requestCookie1);
            HttpContext.Current.Request.Cookies.Add(requestCookie2);

            var returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookies();
            Assert.True(returnCookieData.Count == 2);
            Assert.True(returnCookieData[0].TestContentId == Guid.Parse("997636c1-fec2-43ff-8dd8-fa7f6b3ffa91"));
            Assert.True(returnCookieData[0].Converted);
            Assert.True(returnCookieData[1].TestContentId == Guid.Parse("f222c926-999b-43f9-80dc-667f05e08260"));
            Assert.False(returnCookieData[1].Converted);
        }

  /*
        [Fact]
        public void GetTestDataFromCookie_Resets_Values_For_Inactive_Test()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var expireDate = DateTime.Now.AddDays(2);
            var testVariantId = Guid.NewGuid();

            var testCookie = new HttpCookie("EPI-MAR-" + testContentId.ToString())
            {
                ["TestId"] = _inactiveTestId.ToString(),
                ["ShowVariant"] = "false",
                ["TestContentId"] = testContentId.ToString(),
                ["TestVariantId"] = testVariantId.ToString(),
                ["Viewed"] = "false",
                ["Converted"] = "false",
                Expires = expireDate,
                [Guid.NewGuid().ToString() + "-Flag"] = true.ToString()
            };

            HttpContext.Current.Request.Cookies.Add(testCookie);

            var returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie(testContentId.ToString());
            Assert.True(returnCookieData.TestId == Guid.Empty);
            Assert.True(returnCookieData.TestContentId == Guid.Empty);
            Assert.False(returnCookieData.ShowVariant);
            Assert.True(returnCookieData.TestVariantId == Guid.Empty);
            Assert.False(returnCookieData.Viewed);
            Assert.False(returnCookieData.Converted);
        }
*/
        [Fact]
        public void GetTestDataFromCookie_Returns_EmptyData_When_CookieIsNotAvailable()
        {
            var mockTesteDataCookiehelper = GetUnitUnderTest();
            var invalidTesetContentId = Guid.NewGuid();

            var returnCookieData = mockTesteDataCookiehelper.GetTestDataFromCookie("EPI-MAR-" + invalidTesetContentId.ToString());
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

            var tdCookie = new TestDataCookie()
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

            var originalCookie = new TestDataCookie()
            {
                TestId = _activeTest.Id,
                TestContentId = _testContentGuid,
                TestVariantId = _testVariantGuid,
                ShowVariant = true,
                Viewed = true,
                Converted = false
            };

            var updatedCookie = new TestDataCookie()
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

            var cookieValue = HttpContext.Current.Response.Cookies.Get("EPI-MAR-" + updatedCookie.TestContentId.ToString());

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

            var tdCookie = new TestDataCookie()
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
