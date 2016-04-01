using System;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private IServiceLocator _serviceLocator;

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
            ITestManager _testManager = ServiceLocator.Current.GetInstance<ITestManager>();

            HttpCookie cookieData = new HttpCookie(testData.TestContentId.ToString())
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

        public TestDataCookie GetTestDataFromCookie(string testContentId)
        {
            HttpCookie testDataCookie = HttpContext.Current.Request.Cookies.Get(testContentId);

            if (testDataCookie != null)
            {
                var cookieData = new TestDataCookie()
                {
                    TestId = Guid.Parse(testDataCookie["TestId"]),
                    ShowVariant = bool.Parse(testDataCookie["ShowVariant"]),
                    TestContentId = Guid.Parse(testDataCookie["TestContentId"]),
                    TestParticipant = bool.Parse(testDataCookie["TestParticipant"]),
                    TestVariantId = Guid.Parse(testDataCookie["TestVariantId"]),
                    Viewed = bool.Parse(testDataCookie["Viewed"]),
                    Converted = bool.Parse(testDataCookie["Converted"])

                };
                return cookieData;
            }
            return new TestDataCookie();
        }
    }
}
