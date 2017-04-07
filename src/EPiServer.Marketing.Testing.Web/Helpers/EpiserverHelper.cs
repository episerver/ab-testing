using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Shell;
using EPiServer.Core;
using EPiServer.Web.Routing;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
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
    }
}
