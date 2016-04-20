using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface ITestDataCookieHelper
    {
        bool HasTestData(TestDataCookie testDataCookie);
        bool IsTestParticipant(TestDataCookie testDataCookie);
        void SaveTestDataToCookie(TestDataCookie testData);
        TestDataCookie GetTestDataFromCookie(string testContentId);
        void UpdateTestDataCookie(TestDataCookie testData);
    }
}