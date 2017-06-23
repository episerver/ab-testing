using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Marketing.KPI.Common.Attributes;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(ITestDataCookieHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private IMarketingTestingWebRepository _testRepo;
        private IHttpContextHelper _httpContextHelper;
        internal readonly string COOKIE_PREFIX = "EPI-MAR-";

        [ExcludeFromCodeCoverage]
        public TestDataCookieHelper()
        {
            _testRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _httpContextHelper = new HttpContextHelper();
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        internal TestDataCookieHelper(IMarketingTestingWebRepository testRepo, IHttpContextHelper contextHelper)
        {
            _testRepo = testRepo;
            _httpContextHelper = contextHelper;
        }

        /// <summary>
        /// Evaluates the supplied testdata cookie to determine if it is populated with valid test information
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        public bool HasTestData(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestId != Guid.Empty;
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
            int varIndex = -1;
            if(testData.TestVariantId != Guid.NewGuid())
            {
                varIndex = aTest.Variants.FindIndex(i => i.Id == testData.TestVariantId);
            }
            var cookieData = new HttpCookie(COOKIE_PREFIX + testData.TestContentId.ToString())
            {
                ["TestId"] = testData.TestId.ToString(),
                ["VarIndex"] = varIndex.ToString(),
                ["Viewed"] = testData.Viewed.ToString(),
                ["Converted"] = testData.Converted.ToString(),
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
            _httpContextHelper.RemoveCookie(COOKIE_PREFIX + testData.TestContentId.ToString());
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
            
            if (_httpContextHelper.HasCookie(COOKIE_PREFIX + testContentId))
            {
                cookie = _httpContextHelper.GetResponseCookie(COOKIE_PREFIX + testContentId);
            }
            else
            {
                cookie = _httpContextHelper.GetRequestCookie(COOKIE_PREFIX + testContentId);
            }
           
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                Guid outguid;
                int outint = 0;
                retCookie.TestId = Guid.TryParse(cookie["TestId"], out outguid) ? outguid : Guid.Empty;
                retCookie.TestContentId = Guid.TryParse(cookie.Name.Substring(COOKIE_PREFIX.Length), out outguid) ? outguid : Guid.Empty;

                bool outval;
                retCookie.Viewed = bool.TryParse(cookie["Viewed"], out outval) ? outval : false;
                retCookie.Converted = bool.TryParse(cookie["Converted"], out outval) ? outval : false;

                var test = _testRepo.GetActiveTestsByOriginalItemId(retCookie.TestContentId).FirstOrDefault();
                if (test != null)
                {
                    var index = int.TryParse(cookie["VarIndex"], out outint) ? outint : -1;
                    retCookie.TestVariantId = index != -1 ? test.Variants[outint].Id : Guid.NewGuid();
                    retCookie.ShowVariant = index != -1 ? !test.Variants[outint].IsPublished : false;


                    foreach (var kpi in test.KpiInstances)
                    {
                        bool converted = false;
                        bool.TryParse(cookie[kpi.Id + "-Flag"], out converted);
                        retCookie.KpiConversionDictionary.Add(kpi.Id, converted);
                        retCookie.AlwaysEval = Attribute.IsDefined(kpi.GetType(), typeof(AlwaysEvaluateAttribute));

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
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString();
            _httpContextHelper.RemoveCookie(cookieKey);
            HttpCookie expiredCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId);
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
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString();
            _httpContextHelper.RemoveCookie(cookieKey);
            var resetCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId) {HttpOnly = true};
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
