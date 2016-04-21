using System;
using System.Linq;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private ITestManager _testManager;
        private const string COOKIE_PREFIX = "EPI-MAR-";

        public TestDataCookieHelper()
        {
            _testManager = new TestManager();
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        internal TestDataCookieHelper(ITestManager mockTestManager)
        {
            _testManager = mockTestManager;
        }

        /// <summary>
        /// Evaluates the supplied testdata cookie to determine if it is populated with valid test information
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        public bool HasTestData(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestContentId != Guid.Empty;
        }

        /// <summary>
        /// Evaluates the supplied testdata cookie to determine if the user has been set as a participant.
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        public bool IsTestParticipant(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestVariantId != Guid.Empty;
        }

        /// <summary>
        /// Saves the supplied test data as a cookie
        /// </summary>
        /// <param name="testData"></param>
        public void SaveTestDataToCookie(TestDataCookie testData)
        {
            var cookieData = new HttpCookie(COOKIE_PREFIX + testData.TestContentId.ToString())
            {
                ["TestId"] = testData.TestId.ToString(),
                ["ShowVariant"] = testData.ShowVariant.ToString(),
                ["TestContentId"] = testData.TestContentId.ToString(),
                ["TestVariantId"] = testData.TestVariantId.ToString(),
                ["Viewed"] = testData.Viewed.ToString(),
                ["Converted"] = testData.Converted.ToString(),
                Expires = _testManager.Get(testData.TestId).EndDate.GetValueOrDefault(),
                HttpOnly = true

            };
            HttpContext.Current.Response.Cookies.Add(cookieData);
        }

        /// <summary>
        /// Updates the current cookie
        /// </summary>
        /// <param name="testData"></param>
        public void UpdateTestDataCookie(TestDataCookie testData)
        {
            HttpContext.Current.Response.Cookies.Remove(COOKIE_PREFIX + testData.TestContentId.ToString());
            SaveTestDataToCookie(testData);
        }

        /// <summary>
        /// Gets the current cookie data
        /// </summary>
        /// <param name="testContentId"></param>
        /// <returns></returns>
        public TestDataCookie GetTestDataFromCookie(string testContentId)
        {
            var retCookie = new TestDataCookie();
            var currentContext = HttpContext.Current;
            HttpCookie cookie = currentContext.Response.Cookies.Get(COOKIE_PREFIX + testContentId);
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                cookie = currentContext.Request.Cookies.Get(COOKIE_PREFIX + testContentId);
            }

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                retCookie.TestId = Guid.Parse(cookie["TestId"]);
                retCookie.ShowVariant = bool.Parse(cookie["ShowVariant"]);
                retCookie.TestContentId = Guid.Parse(cookie["TestContentId"]);
                retCookie.TestVariantId = Guid.Parse(cookie["TestVariantId"]);
                retCookie.Viewed = bool.Parse(cookie["Viewed"]);
                retCookie.Converted = bool.Parse(cookie["Converted"]);
            }
            return retCookie;
        }

        /// <summary>
        /// Sets the cookie associated with the supplied testData to expire
        /// </summary>
        /// <param name="testData"></param>
        public void ExpireTestDataCookie(TestDataCookie testData)
        {
            HttpContext.Current.Response.Cookies.Remove(COOKIE_PREFIX + testData.TestContentId.ToString());
            HttpCookie expiredCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId);
            expiredCookie.HttpOnly = true;
            expiredCookie.Expires = DateTime.Now.AddDays(-1d);
            HttpContext.Current.Response.Cookies.Add(expiredCookie);
        }
    }
}
