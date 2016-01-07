using System.Configuration;
using EPiServer.Data.Configuration;

namespace EPiServer.Marketing.Multivariate
{
    public class CurrentSite : ICurrentSite
    {
        public string GetSiteDataBaseConnectionString()
        {
            var siteSettings = new SiteDataSettingsElement();
            var connectionStringName = siteSettings.ConnectionStringName;
            return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
        }
    }
}
