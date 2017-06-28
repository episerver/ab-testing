using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Marketing.KPI.Common.Attributes;
using System.Globalization;

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
            if (testData.TestVariantId != Guid.NewGuid())
            {
                varIndex = aTest.Variants.FindIndex(i => i.Id == testData.TestVariantId);
            }
            var cookieData = new HttpCookie(COOKIE_PREFIX + testData.TestContentId.ToString() + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo().Name)
            {
                ["start"] = aTest.StartDate.ToString(),
                ["vId"] = varIndex.ToString(),
                ["viewed"] = testData.Viewed.ToString(),
                ["converted"] = testData.Converted.ToString(),
                Expires = aTest.EndDate,
                HttpOnly = true

            };
            testData.KpiConversionDictionary.OrderBy(x => x.Key);
            for (var x = 0; x < testData.KpiConversionDictionary.Count; x++)
            {
                cookieData["k" + x] = testData.KpiConversionDictionary.ToList()[x].Value.ToString();
            }

            _httpContextHelper.AddCookie(cookieData);
        }

        /// <summary>
        /// Updates the current cookie
        /// </summary>
        /// <param name="testData"></param>
        public void UpdateTestDataCookie(TestDataCookie testData)
        {
            _httpContextHelper.RemoveCookie(COOKIE_PREFIX + testData.TestContentId.ToString() + COOKIE_DELIMETER + _episerverHelper.GetContentCultureinfo().Name);
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
            var currentCulture = _episerverHelper.GetContentCultureinfo();
            var cultureName = currentCulture.Name;

            if (_httpContextHelper.HasCookie(COOKIE_PREFIX + testContentId + COOKIE_DELIMETER + cultureName))
            {
                cookie = _httpContextHelper.GetResponseCookie(COOKIE_PREFIX + testContentId + COOKIE_DELIMETER + cultureName);
            }
            else
            {
                cookie = _httpContextHelper.GetRequestCookie(COOKIE_PREFIX + testContentId + COOKIE_DELIMETER + cultureName);
            }

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                if (!cookie.Values.Keys.ToString().Contains("tld"))
                {
                    Guid outguid;
                    int outint = 0;
                    retCookie.TestId = Guid.TryParse(cookie["tId"], out outguid) ? outguid : Guid.Empty;
                    retCookie.TestContentId = Guid.TryParse(cookie.Name.Substring(COOKIE_PREFIX.Length).Split(':')[0], out outguid) ? outguid : Guid.Empty;

                    bool outval;
                    retCookie.Viewed = bool.TryParse(cookie["viewed"], out outval) ? outval : false;
                    retCookie.Converted = bool.TryParse(cookie["converted"], out outval) ? outval : false;

                    ABTestFilter filter = new ABTestFilter()
                    {
                        Operator = FilterOperator.And,
                        Property = ABTestProperty.OriginalItemId,
                        Value = retCookie.TestContentId
                    };

                    TestCriteria criteria = new TestCriteria();
                    criteria.AddFilter(filter);


                    var test = _testRepo.GetTestList(criteria).Where(d => d.StartDate.ToString() == cookie["start"]).FirstOrDefault();
                    if (test != null)
                    {
                        var index = int.TryParse(cookie["vId"], out outint) ? outint : -1;
                        retCookie.TestVariantId = index != -1 ? test.Variants[outint].Id : Guid.NewGuid();
                        retCookie.ShowVariant = index != -1 ? !test.Variants[outint].IsPublished : false;
                        retCookie.TestId = test.Id;

                        var orderedKpiInstances = test.KpiInstances.OrderBy(x => x.Id).ToList();
                        test.KpiInstances = orderedKpiInstances;

                        for (var x = 0; x < test.KpiInstances.Count; x++)
                        {
                            bool converted = false;
                            bool.TryParse(cookie["k" + x], out converted);
                            retCookie.KpiConversionDictionary.Add(test.KpiInstances[x].Id, converted);
                            retCookie.AlwaysEval = Attribute.IsDefined(test.KpiInstances[x].GetType(), typeof(AlwaysEvaluateAttribute));
                        }
                    }
                }
                else
                {
                    retCookie = GetTestDataFromOldCookie(cookie);
                }
            }
            return retCookie;
        }

        /// <summary>
        /// This method converts cookies which contain a language indicator but are in the long key format to the shorthand format.
        /// This is to support installs which may be using the long hand key format so as not to inturrupt or break their tests.
        /// We can remove this after a couple of revs once it is no longer a concern.
        /// </summary>
        /// <param name="testContentId"></param>
        /// <param name="cultureName"></param>
        /// <returns></returns>
        internal TestDataCookie GetTestDataFromOldCookie(HttpCookie cookieToConvert)
        {
            var retCookie = new TestDataCookie();

            Guid outguid;
            CultureInfo culture = new CultureInfo(cookieToConvert.Name.Split(':')[1]);
            retCookie.TestId = Guid.TryParse(cookieToConvert["TestId"], out outguid) ? outguid : Guid.Empty;
            retCookie.TestContentId = Guid.TryParse(cookieToConvert["TestContentId"], out outguid) ? outguid : Guid.Empty;
            retCookie.TestVariantId = Guid.TryParse(cookieToConvert["TestVariantId"], out outguid) ? outguid : Guid.Empty;

            bool outval;
            retCookie.ShowVariant = bool.TryParse(cookieToConvert["ShowVariant"], out outval) ? outval : false;
            retCookie.Viewed = bool.TryParse(cookieToConvert["Viewed"], out outval) ? outval : false;
            retCookie.Converted = bool.TryParse(cookieToConvert["Converted"], out outval) ? outval : false;
            retCookie.AlwaysEval = bool.TryParse(cookieToConvert["AlwaysEval"], out outval) ? outval : false;

            var test = _testRepo.GetActiveTestsByOriginalItemId(retCookie.TestContentId, culture).FirstOrDefault();
            if (test != null)
            {
                foreach (var kpi in test.KpiInstances)
                {
                    bool converted = false;
                    bool.TryParse(cookieToConvert[kpi.Id + "-Flag"], out converted);
                    retCookie.KpiConversionDictionary.Add(kpi.Id, converted);
                }
            }
            UpdateTestDataCookie(retCookie);
            return retCookie;
        }

        /// <summary>
        /// Sets the cookie associated with the supplied testData to expire
        /// </summary>
        /// <param name="testData"></param>
        public void ExpireTestDataCookie(TestDataCookie testData)
        {
            var cultureName = _episerverHelper.GetContentCultureinfo().Name;
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString() + COOKIE_DELIMETER + cultureName;
            _httpContextHelper.RemoveCookie(cookieKey);
            HttpCookie expiredCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId + COOKIE_DELIMETER + cultureName);
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
            var cultureName = _episerverHelper.GetContentCultureinfo().Name;
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString() + COOKIE_DELIMETER + cultureName;
            _httpContextHelper.RemoveCookie(cookieKey);
            var resetCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId + COOKIE_DELIMETER + cultureName) { HttpOnly = true };
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
                                            select GetTestDataFromCookie(name.Split(':')[0].Substring(COOKIE_PREFIX.Length))).ToList();

            //Get cookie data from cookies not recently updated.
            tdcList.AddRange(from name in _httpContextHelper.GetRequestCookieKeys()
                             where name.Contains(COOKIE_PREFIX) &&
                             !aResponseCookieKeys.Contains(name)
                             select GetTestDataFromCookie(name.Split(':')[0].Substring(COOKIE_PREFIX.Length)));

            return tdcList;
        }
    }
}
