
using System.Diagnostics.CodeAnalysis;
using System.Web;
using EPiServer.Core;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestingContextHelper : ITestingContextHelper
    {
        public TestingContextHelper()
        {}

        /// <summary>
        /// For Unit Testing
        /// </summary>
        /// <param name="context"></param>
        [ExcludeFromCodeCoverage]
        internal TestingContextHelper(HttpContext context)
        {
            HttpContext.Current = context;
        }
        /// <summary>
        /// Evaluates current URL to determine if page is in a system folder context (e.g Edit, or Preview)
        /// </summary>
        /// <returns></returns>
        public bool IsInSystemFolder()
        {
            var currentContext = HttpContext.Current;
            if (currentContext == null)
            {
                return true;
            }

            return currentContext.Request.RawUrl.ToLower()
                .Contains(EPiServer.Shell.Paths.ProtectedRootPath.ToLower());
        }

        /// <summary>
        /// Compares the current content with the requested content
        /// </summary>
        /// <param name="requestedContent"></param>
        /// <param name="loadedContent"></param>
        /// <returns></returns>
        public bool IsRequestedContent(IContent requestedContent, IContent loadedContent)
        {
            return requestedContent.ContentLink == loadedContent.ContentLink;
        }

        /// <summary>
        /// Converts the current URL to an IContent object
        /// </summary>
        /// <returns></returns>
        public IContent GetCurrentPageFromUrl()
        {
            return HttpContext.Current.Items["CurrentPage"] as IContent;
        }
    }
}
