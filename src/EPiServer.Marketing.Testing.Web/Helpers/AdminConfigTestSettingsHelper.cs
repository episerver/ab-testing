using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(IAdminConfigTestSettingsHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class AdminConfigTestSettingsHelper : IAdminConfigTestSettingsHelper
    {
        public string GetCookieDelimeter()
        {
            return AdminConfigTestSettings.Current.CookieDelimeter;
        }
    }
}
