using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ServiceConfiguration(typeof(ViewConfiguration))]
    [ExcludeFromCodeCoverage]
    public class ArchiveViewConfiguration : ViewConfiguration<IMarketingTest>
    {
        public ArchiveViewConfiguration()
        {
            Key = "ArchiveView";
            ControllerType = "marketing-testing/views/Archive";
            ViewType = "marketing-testing/views/Archive";
            HideFromViewMenu = true;
        }
    }
}
