using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Data;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ServiceConfiguration(typeof(ViewConfiguration))]
    [ExcludeFromCodeCoverage]
    public class MarketingTestPickWinnerViewConfiguration : ViewConfiguration<IMarketingTest>
    {

        public MarketingTestPickWinnerViewConfiguration()
        {
            Key = "MarketingTestPickWinnerView";
            ControllerType = "marketing-testing/views/MarketingTestPickWinnerView";
            ViewType = "marketing-testing/views/MarketingTestPickWinnerView";
            HideFromViewMenu = true;

        }
    }
}
