using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Shell;
using EPiServer.Core;
using EPiServer.Web.Routing;
using System.Globalization;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(IEpiserverHelper), Lifecycle = ServiceInstanceScope.Singleton)]

    internal class EpiserverHelper : IEpiserverHelper
    {
        public string GetRootPath()
        {
            return Paths.ProtectedRootPath;
        }

        public string GetPreviewUrl(ContentReference cr, string language, VirtualPathArguments args)
        {
            return UrlResolver.Current.GetUrl(cr, language, args);
        }

        public CultureInfo GetContentCultureinfo()
        {
            return ContentLanguage.PreferredCulture;
        }
    }
}
