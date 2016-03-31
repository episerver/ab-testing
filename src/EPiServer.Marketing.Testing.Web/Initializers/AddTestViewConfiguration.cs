using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace EPiServer.Marketing.Testing.Web
{
    [ServiceConfiguration(typeof(ViewConfiguration))]
    public class AddTestViewConfiguration : ViewConfiguration<IContentData>
    {
        public AddTestViewConfiguration()
        {
            Key = "AddTestView";
            ControllerType = "marketing-testing/views/AddTestView";
            ViewType = "marketing-testing/views/AddTestView";
            //IconClass;
            //HideFromViewMenu;
        }
    }
}