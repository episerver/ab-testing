using System.Globalization;
using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestDataCookieMigrator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookiePrefix"></param>
        /// <param name="testContentId"></param>
        /// <param name="cultureName"></param>
        /// <returns></returns>
        TestDataCookie UpdateOldCookie(string cookiePrefix, string testContentId, CultureInfo currentCulture, string currentCulturename);
    }
}
