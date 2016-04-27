using EPiServer.Marketing.Testing.Core.DataClass;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface ITestDataCookieHelper
    {
        bool HasTestData(TestDataCookie testDataCookie);
        bool IsTestParticipant(TestDataCookie testDataCookie);
        void SaveTestDataToCookie(TestDataCookie testData);
        TestDataCookie GetTestDataFromCookie(string testContentId);
        void UpdateTestDataCookie(TestDataCookie testData);

        /// <summary>
        /// Finds and returns a list of all testing cookies objects
        /// </summary>
        /// <returns>can be empty if there are no test cookies, never null</returns>
        IList<TestDataCookie> getTestDataFromCookies();
    }
}