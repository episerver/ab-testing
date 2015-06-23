using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Data.Configuration;

namespace EPiServer.Marketing.Multivariate.Core.Cms
{
    public class CurrentSite : ICurrentSite
    {
        public string GetSiteDataBaseConnectionString()
        {
            var siteSettings = new SiteDataSettingsElement();
            return siteSettings.ConnectionStringName;
        }
    }
}
