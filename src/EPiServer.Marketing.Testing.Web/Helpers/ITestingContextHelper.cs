using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Models;
using System;
using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface ITestingContextHelper
    {
        bool IsRequestedContent(IContent loadedContent);
        IContent GetCurrentPage();
        bool SwapDisabled(EventArgs e);
        bool SwapDisabled(ChildrenEventArgs e);
        MarketingTestingContextModel GenerateContextData(IMarketingTest testData);
        bool IsInSystemFolder();
        bool IsHtmlContentType();

    }
}