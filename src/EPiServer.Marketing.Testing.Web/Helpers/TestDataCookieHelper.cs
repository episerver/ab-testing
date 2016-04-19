using System;
using System.Collections.Generic;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using System.Data.Entity.Core;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private ITestManager _testManager;

        public TestDataCookieHelper()
        {
            _testManager = new TestManager();
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        /// <param name="locator"></param>
        internal TestDataCookieHelper(ITestManager mockTestManager)
        {
            _testManager = mockTestManager;
        }


        public bool HasTestData(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestContentId != Guid.Empty;
        }

        public bool IsTestParticipant(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestVariantId != Guid.Empty;
        }

        public void SaveTestDataToCookie(TestDataCookie testData)
        {

            var cookieData = new HttpCookie("EPI-MAR-"+testData.TestContentId.ToString())
            {
                ["TestId"] = testData.TestId.ToString(),
                ["ShowVariant"] = testData.ShowVariant.ToString(),
                ["TestContentId"] = testData.TestContentId.ToString(),
                ["TestVariantId"] = testData.TestVariantId.ToString(),
                ["Viewed"] = testData.Viewed.ToString(),
                ["Converted"] = testData.Converted.ToString(),
                Expires = _testManager.Get(testData.TestId).EndDate.GetValueOrDefault()
            };
            foreach (var kpi in testData.KpiConversionDictionary)
            {
                cookieData[kpi.Key.ToString() + "-Flag"] = kpi.Value.ToString();
            }

            HttpContext.Current.Response.Cookies.Add(cookieData);
        }

        public void UpdateTestDataCookie(TestDataCookie testData)
        {
            HttpContext.Current.Response.Cookies.Remove("EPI-MAR-" + testData.TestContentId.ToString());
            SaveTestDataToCookie(testData);
        }

        public TestDataCookie GetTestDataFromCookie(string testContentId)
        {
            var retCookie = new TestDataCookie();
            var testDataCookie = HttpContext.Current.Request.Cookies.Get("EPI-MAR-" + testContentId);

            if (testDataCookie != null)
            {
                retCookie.TestId = Guid.Parse(testDataCookie["TestId"]);
                retCookie.ShowVariant = bool.Parse(testDataCookie["ShowVariant"]);
                retCookie.TestContentId = Guid.Parse(testDataCookie["TestContentId"]);
                retCookie.TestVariantId = Guid.Parse(testDataCookie["TestVariantId"]);
                retCookie.Viewed = bool.Parse(testDataCookie["Viewed"]);
                retCookie.Converted = bool.Parse(testDataCookie["Converted"]);

                try
                {
                    var t = _testManager.Get(retCookie.TestId);
                    foreach (var kpi in t.KpiInstances)
                    {
                        bool converted = bool.Parse(testDataCookie[kpi.Id + "-Flag"]);
                        retCookie.KpiConversionDictionary.Add(kpi.Id, converted);
                    }
                }
                catch (ObjectNotFoundException)
                {
                    // test doesnt exist but this user had a cookie for it so delete the cookie
                    HttpCookie delCookie = new HttpCookie("EPI-MAR-" + testContentId);
                    delCookie.Expires = DateTime.Now.AddDays(-1d);
                    HttpContext.Current.Response.Cookies.Add(delCookie);
                }
            }

            return retCookie;
        }

        public IList<TestDataCookie> getTestDataFromCookies()
        {
            List<TestDataCookie> tdcList = new List<TestDataCookie>();

            foreach( var name in HttpContext.Current.Request.Cookies.AllKeys )
            {
                if( name.Contains("EPI-MAR-") )
                {
                    tdcList.Add(GetTestDataFromCookie(name.Substring("EPI-MAR-".Length)));
                }
            }

            return tdcList;
        }
    }
}
