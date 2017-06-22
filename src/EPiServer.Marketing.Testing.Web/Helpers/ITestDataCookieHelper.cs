using EPiServer.Marketing.Testing.Core.DataClass;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface ITestDataCookieHelper
    {
        /// <summary>
        /// Checks cookie for test data
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        bool HasTestData(TestDataCookie testDataCookie);

        /// <summary>
        /// Returns true if cookie variant ID is supplied
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        bool IsTestParticipant(TestDataCookie testDataCookie);

        /// <summary>
        /// Saves test data to the response
        /// </summary>
        /// <param name="testData"></param>
        void SaveTestDataToCookie(TestDataCookie testData);
       
        /// <summary>
        /// Finds and returns a test data cookie associated with the content
        /// </summary>
        /// <param name="testContentId"></param>
        /// <returns></returns>
        TestDataCookie GetTestDataFromCookie(string testContentId, string cultureName = null);

        /// <summary>
        /// Finds and returns a list of all testing cookies objects
        /// </summary>
        /// <returns>can be empty if there are no test cookies, never null</returns>
        IList<TestDataCookie> GetTestDataFromCookies();

        /// <summary>
        /// Removes and replaces existing cookie data with new test data
        /// </summary>
        /// <param name="testData"></param>
        void UpdateTestDataCookie(TestDataCookie testData);

        /// <summary>
        /// Sets the specified cookie to expire immediately
        /// </summary>
        /// <param name="testData"></param>
        void ExpireTestDataCookie(TestDataCookie testData);

        // <summary>
        /// Resets the cookie associated with the supplied testData to an empty Test Data cookie.
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        TestDataCookie ResetTestDataCookie(TestDataCookie testData);
    }
}