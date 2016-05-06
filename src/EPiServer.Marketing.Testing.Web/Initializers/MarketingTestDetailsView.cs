using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Data;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace EPiServer.Marketing.Testing.Web
{
    [ServiceConfiguration(typeof(ViewConfiguration))]
    [ExcludeFromCodeCoverage]
    public class MultivariateTestDetailsViewConfiguration : ViewConfiguration<IMarketingTest>
    {
        public MultivariateTestDetailsViewConfiguration()
        {
            Key = "MultivariateTestDetailsView";
            ControllerType = "marketing-testing/views/MarketingTestDetailsView";
            ViewType = "marketing-testing/views/MarketingTestDetailsView";
        }
    }
}
