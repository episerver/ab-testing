using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Models;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface ITestingContextHelper
    {
        bool IsInSystemFolder();
        bool IsRequestedContent(IContent loadedContent);
        IContent GetCurrentPageFromUrl();
        MarketingTestingContextModel GenerateContextData(IMarketingTest testData);

    }
}