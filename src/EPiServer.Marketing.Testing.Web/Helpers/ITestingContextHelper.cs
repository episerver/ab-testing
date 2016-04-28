using EPiServer.Core;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface ITestingContextHelper
    {
        bool IsInSystemFolder();
        bool IsRequestedContent(IContent requestedContent, IContent loadedContent);
        IContent GetCurrentPageFromUrl();
    }
}