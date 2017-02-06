using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace EPiServer.Marketing.Testing.Web
{
    [ServiceConfiguration(typeof(ViewConfiguration))]
    [ExcludeFromCodeCoverage]
    public class DetailsViewConfiguration : ViewConfiguration<IMarketingTest>
    {
        public DetailsViewConfiguration()
        {
            Key = "DetailsView";
            ControllerType = "marketing-testing/views/Details";
            ViewType = "marketing-testing/views/Details";
            HideFromViewMenu = true;

        }
    }
}
