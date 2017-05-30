using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Repositories;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(ITestDataCookieHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private IMarketingTestingWebRepository _testRepo;
        private IHttpContextHelper _httpContextHelper;
        private IEpiserverHelper _episerverHelper;

        internal readonly string COOKIE_PREFIX = "EPI-MAR-";
        internal readonly string COOKIE_DELIMETER = ":";


        [ExcludeFromCodeCoverage]
        public TestDataCookieHelper()
        {
            _testRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            _httpContextHelper = new HttpContextHelper();
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        internal TestDataCookieHelper(IMarketingTestingWebRepository testRepo, IHttpContextHelper contextHelper, IEpiserverHelper epiHelper)
        {
            _testRepo = testRepo;
            _httpContextHelper = contextHelper;
            _episerverHelper = epiHelper;
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
            var aTest = _testRepo.GetTestById(testData.TestId);
            var cookieData = new HttpCookie(COOKIE_PREFIX + testData.TestContentId.ToString() + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo())
            {
                ["TestId"] = testData.TestId.ToString(),
                ["ShowVariant"] = testData.ShowVariant.ToString(),
                ["TestContentId"] = testData.TestContentId.ToString(),
                ["TestVariantId"] = testData.TestVariantId.ToString(),
                ["Viewed"] = testData.Viewed.ToString(),
                ["Converted"] = testData.Converted.ToString(),
                ["AlwaysEval"] = testData.AlwaysEval.ToString(),
                Expires = aTest.EndDate,
                HttpOnly = true

            };
            foreach (var kpi in testData.KpiConversionDictionary)
            {
                cookieData[kpi.Key.ToString() + "-Flag"] = kpi.Value.ToString();
            }

            _httpContextHelper.AddCookie(cookieData);
        }

        /// <summary>
        /// Updates the current cookie
        /// </summary>
        /// <param name="testData"></param>
        public void UpdateTestDataCookie(TestDataCookie testData)
        {
            _httpContextHelper.RemoveCookie(COOKIE_PREFIX + testData.TestContentId.ToString() + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo());
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
            HttpCookie cookie;
            
            if (_httpContextHelper.HasCookie(COOKIE_PREFIX + testContentId + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo()))
            {
                cookie = _httpContextHelper.GetResponseCookie(COOKIE_PREFIX + testContentId + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo());
            }
            else
            {
                cookie = _httpContextHelper.GetRequestCookie(COOKIE_PREFIX + testContentId + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo());
            }
           
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                Guid outguid;
                retCookie.TestId = Guid.TryParse(cookie["TestId"], out outguid) ? outguid : Guid.Empty;
                retCookie.TestContentId = Guid.TryParse(cookie["TestContentId"], out outguid) ? outguid : Guid.Empty;
                retCookie.TestVariantId = Guid.TryParse(cookie["TestVariantId"], out outguid) ? outguid : Guid.Empty;

                bool outval;
                retCookie.ShowVariant = bool.TryParse(cookie["ShowVariant"], out outval) ? outval : false;
                retCookie.Viewed = bool.TryParse(cookie["Viewed"], out outval) ? outval : false;
                retCookie.Converted = bool.TryParse(cookie["Converted"], out outval) ? outval : false;
                retCookie.AlwaysEval = bool.TryParse(cookie["AlwaysEval"], out outval) ? outval : false;

                var test = _testRepo.GetActiveTestsByOriginalItemId(retCookie.TestContentId).FirstOrDefault();
                if (test != null)
                {
                    foreach (var kpi in test.KpiInstances)
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
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString() + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo();
            _httpContextHelper.RemoveCookie(cookieKey);
            HttpCookie expiredCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo());
            expiredCookie.HttpOnly = true;
            expiredCookie.Expires = DateTime.Now.AddDays(-1d);
            _httpContextHelper.AddCookie(expiredCookie);
        }

        /// <summary>
        /// Resets the cookie associated with the supplied testData to an empty Test Data cookie.
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public TestDataCookie ResetTestDataCookie(TestDataCookie testData)
        {
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString() + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo();
            _httpContextHelper.RemoveCookie(cookieKey);
            var resetCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo()) {HttpOnly = true};
            _httpContextHelper.AddCookie(resetCookie);
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
            var aResponseCookieKeys = _httpContextHelper.GetResponseCookieKeys();
            List<TestDataCookie> tdcList = (from name in aResponseCookieKeys
                                            where name.Contains(COOKIE_PREFIX)
                                            select GetTestDataFromCookie(name.Substring(COOKIE_PREFIX.Length))).ToList();

            //Get cookie data from cookies not recently updated.
            tdcList.AddRange(from name in _httpContextHelper.GetRequestCookieKeys()
                             where name.Contains(COOKIE_PREFIX) &&
                             !aResponseCookieKeys.Contains(name)
                             select GetTestDataFromCookie(name.Substring(COOKIE_PREFIX.Length)));

            return tdcList;
        }
    }
}
