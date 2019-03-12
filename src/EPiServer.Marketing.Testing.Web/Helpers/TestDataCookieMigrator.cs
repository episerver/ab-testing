using System;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestDataCookieMigrator : ITestDataCookieMigrator
    {
        private IHttpContextHelper _httpContextHelper;
        private IMarketingTestingWebRepository _testRepo;
        private IEpiserverHelper _episerverHelper;
        private string _oldCookieDelimeter = ":";

        /// <summary>
        /// 
        /// </summary>
        public TestDataCookieMigrator()
        {
            _testRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            _httpContextHelper = new HttpContextHelper();
        }

        internal TestDataCookieMigrator(IMarketingTestingWebRepository marketingTestingWebRepository, IEpiserverHelper episerverHelper, IHttpContextHelper httpContextHelper)
        {
            _testRepo = marketingTestingWebRepository;
            _episerverHelper = episerverHelper;
            _httpContextHelper = httpContextHelper;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookiePrefix"></param>
        /// <param name="testContentId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="currentCulturename"></param>
        /// <returns></returns>
        public TestDataCookie UpdateOldCookie(string cookiePrefix, string testContentId, CultureInfo currentCulture, string currentCulturename)
        {
            TestDataCookie tdCookie = null;
            var cookieKey = cookiePrefix + testContentId + _oldCookieDelimeter + currentCulturename;
            var cookie = _httpContextHelper.HasCookie(cookieKey)
                ? _httpContextHelper.GetResponseCookie(cookieKey)
                : _httpContextHelper.GetRequestCookie(cookieKey);

            if (cookie != null)
            {
                Guid outguid;
                var contentId = Guid.TryParse(cookie.Name.Substring(cookiePrefix.Length).Split(_oldCookieDelimeter[0])[0], out outguid) ? outguid : Guid.Empty;
                var test = _testRepo.GetActiveTestsByOriginalItemId(contentId, currentCulture).FirstOrDefault();
                var startDate = DateTime.Parse(cookie["start"], CultureInfo.InvariantCulture);

                if (test != null &&
                    startDate.ToString(CultureInfo.InvariantCulture) == test.StartDate.ToString(CultureInfo.InvariantCulture))
                {
                    var outint = 0;
                    var index = int.TryParse(cookie["vId"], out outint) ? outint : -1;
                    bool outval;
                    tdCookie = new TestDataCookie
                    {
                        TestContentId = Guid.Parse(testContentId),
                        TestId = test.Id,
                        TestVariantId = index != -1 ? test.Variants[outint].Id : Guid.Empty,
                        TestStart = startDate,
                        ShowVariant = index != -1 && !test.Variants[outint].IsPublished,
                        Viewed = bool.TryParse(cookie["viewed"], out outval) && outval,
                        Converted = bool.TryParse(cookie["converted"], out outval) && outval
                    };

                    var orderedKpiInstances = test.KpiInstances.OrderBy(x => x.Id).ToList();
                    test.KpiInstances = orderedKpiInstances;

                    for (var x = 0; x < test.KpiInstances.Count; x++)
                    {
                        bool converted;
                        bool.TryParse(cookie["k" + x], out converted);
                        tdCookie.KpiConversionDictionary.Add(test.KpiInstances[x].Id, converted);
                        tdCookie.AlwaysEval = Attribute.IsDefined(test.KpiInstances[x].GetType(), typeof(AlwaysEvaluateAttribute));
                    }
                }
            }

            return tdCookie;
        }
    }
}
