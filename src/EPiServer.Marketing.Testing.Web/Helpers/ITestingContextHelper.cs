using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Models;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface ITestingContextHelper
    {
        bool IsRequestedContent(IContent loadedContent);
        IContent GetCurrentPageFromUrl();
        bool SwapDisabled(ContentEventArgs e);
        bool SwapDisabled(ChildrenEventArgs e);
        MarketingTestingContextModel GenerateContextData(IMarketingTest testData);

    }
}