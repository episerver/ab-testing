using System;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private IServiceLocator _serviceLocator;
        private ITestManager _testManager;

        public TestDataCookieHelper()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        /// <param name="locator"></param>
        internal TestDataCookieHelper(IServiceLocator locator)
        {
            _serviceLocator = locator;
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
            _testManager = _serviceLocator.GetInstance<ITestManager>();

            var cookieData = new HttpCookie(testData.TestContentId.ToString())
            {
                ["TestId"] = testData.TestId.ToString(),
                ["ShowVariant"] = testData.ShowVariant.ToString(),
                ["TestContentId"] = testData.TestContentId.ToString(),
                ["TestParticipant"] = testData.TestParticipant.ToString(),
                ["TestVariantId"] = testData.TestVariantId.ToString(),
                ["Viewed"] = testData.Viewed.ToString(),
                ["Converted"] = testData.Converted.ToString(),
                Expires = _testManager.Get(testData.TestId).EndDate.GetValueOrDefault()
            };
            HttpContext.Current.Response.Cookies.Add(cookieData);
        }

        public void UpdateTestDataCookie(TestDataCookie testData)
        {
            HttpContext.Current.Response.Cookies.Remove(testData.TestContentId.ToString());
            SaveTestDataToCookie(testData);
        }

        public TestDataCookie GetTestDataFromCookie(string testContentId)
        {
            var retCookie = new TestDataCookie();
            var testDataCookie = HttpContext.Current.Request.Cookies.Get(testContentId);

            if (testDataCookie != null)
            {
                retCookie.TestId = Guid.Parse(testDataCookie["TestId"]);
                retCookie.ShowVariant = bool.Parse(testDataCookie["ShowVariant"]);
                retCookie.TestContentId = Guid.Parse(testDataCookie["TestContentId"]);
                retCookie.TestParticipant = bool.Parse(testDataCookie["TestParticipant"]);
                retCookie.TestVariantId = Guid.Parse(testDataCookie["TestVariantId"]);
                retCookie.Viewed = bool.Parse(testDataCookie["Viewed"]);
                retCookie.Converted = bool.Parse(testDataCookie["Converted"]);
            }

            return retCookie;
        }
    }
}
