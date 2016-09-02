using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private ITestManager _testManager;
        private const string COOKIE_PREFIX = "EPI-MAR-";

        [ExcludeFromCodeCoverage]
        public TestDataCookieHelper()
        {
            _testManager = ServiceLocator.Current.GetInstance<ITestManager>();
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
            foreach (var kpi in testData.KpiConversionDictionary)
            {
                cookieData[kpi.Key.ToString() + "-Flag"] = kpi.Value.ToString();
            }

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
            HttpCookie cookie;

            if (currentContext.Response.Cookies.AllKeys.Contains(COOKIE_PREFIX + testContentId))
            {
                cookie = currentContext.Response.Cookies.Get(COOKIE_PREFIX + testContentId);
            }
            else
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

                var t = _testManager.GetActiveTestsByOriginalItemId(retCookie.TestContentId).FirstOrDefault();
                if (t != null)
                {
                    foreach (var kpi in t.KpiInstances)
                    {
                        bool converted = false;
                        bool.TryParse(cookie[kpi.Id + "-Flag"], out converted);
                        retCookie.KpiConversionDictionary.Add(kpi.Id, converted);
                    }
                }
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
            HttpContext.Current.Request.Cookies.Remove(COOKIE_PREFIX + testData.TestContentId.ToString());
            HttpCookie expiredCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId);
            expiredCookie.HttpOnly = true;
            expiredCookie.Expires = DateTime.Now.AddDays(-1d);
            HttpContext.Current.Response.Cookies.Add(expiredCookie);
        }

        /// <summary>
        /// Resets the cookie associated with the supplied testData to an empty Test Data cookie.
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public TestDataCookie ResetTestDataCookie(TestDataCookie testData)
        {
            HttpContext.Current.Response.Cookies.Remove(COOKIE_PREFIX + testData.TestContentId.ToString());
            HttpContext.Current.Request.Cookies.Remove(COOKIE_PREFIX + testData.TestContentId.ToString());
            HttpCookie resetCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId) {HttpOnly = true};
            HttpContext.Current.Response.Cookies.Add(resetCookie);
            return new TestDataCookie();
        }

        /// <summary>
        /// Gets test cookie data from both Response and Request.
        /// Fetching response cookies gets current cookie data for cookies actively being processed
        /// while fetching request cookies gets cookie data for cookies which have not been touched.
        /// This ensure a complete set of current cookie data and prevents missed views or duplicated conversions.
        /// </summary>
        /// <returns></returns>
        public IList<TestDataCookie> GetTestDataFromCookies()
        {
            //Get up to date cookies data for cookies which are actively being processed
            List<TestDataCookie> tdcList = (from name in HttpContext.Current.Response.Cookies.AllKeys
                                            where name.Contains(COOKIE_PREFIX)
                                            select GetTestDataFromCookie(name.Substring(COOKIE_PREFIX.Length))).ToList();

            //Get cookie data from cookies not recently updated.
            tdcList.AddRange(from name in HttpContext.Current.Request.Cookies.AllKeys
                             where name.Contains(COOKIE_PREFIX) &&
                             !HttpContext.Current.Response.Cookies.AllKeys.Contains(name)
                             select GetTestDataFromCookie(name.Substring(COOKIE_PREFIX.Length)));

            return tdcList;
        }
    }
}
