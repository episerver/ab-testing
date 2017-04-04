using EPiServer.Core;
using EPiServer.Web.Routing;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// Encapsulates all the items within the Episerver CMS API that can not be mocked on its own. Static items, classes without an interface, etc.
    /// </summary>
    internal interface IEpiserverHelper
    {
        /// <summary>
        /// Returns the Episerver root CMS Path
        /// </summary>
        /// <returns>the path string</returns>
        string GetRootPath();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cr"></param>
        /// <param name="language"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        string GetPreviewUrl(ContentReference cr, string language, VirtualPathArguments args);
    }
}
