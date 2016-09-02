using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Data;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ServiceConfiguration(typeof(ViewConfiguration))]
    [ExcludeFromCodeCoverage]
    public class PickWinnerViewConfiguration : ViewConfiguration<IMarketingTest>
    {
        public PickWinnerViewConfiguration()
        {
            Key = "PickWinnerView";
            ControllerType = "marketing-testing/views/PickWinner";
            ViewType = "marketing-testing/views/PickWinner";
            HideFromViewMenu = true;
        }
    }
}
