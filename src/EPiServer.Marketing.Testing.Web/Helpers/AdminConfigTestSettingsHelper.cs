using EPiServer.Marketing.Testing.Web.Config;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class AdminConfigTestSettingsHelper : IAdminConfigTestSettingsHelper
    {
        public string GetCookieDelimeter()
        {
            return AdminConfigTestSettings.Current.CookieDelimeter;
        }
    }
}
