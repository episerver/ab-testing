using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Routing;

namespace EPiServer.Marketing.KPI.Common
{
    public class LandingPageKPI : Kpi
    {
        private Guid _PageGuid;
        private VirtualPathData _PagePath;
        private IServiceLocator _locator;

        [ExcludeFromCodeCoverage]
        public LandingPageKPI(Guid pageGuid)
        {
            Properties = pageGuid.ToString();
            _locator = ServiceLocator.Current;
        }

        internal LandingPageKPI(Guid pageGuid, IServiceLocator locator)
        {
            Properties = pageGuid.ToString();
            _locator = locator;
        }

        public Boolean Evaluate(String RequestFilePath)
        {
            // Make sure we have the guid to look for, if not we will parse it only once 
            // from the properties.
            if (Guid.Empty == _PageGuid)
            {
                _PageGuid = Guid.Parse(Properties);
            }

            // figure out the page path only once.
            if (_PagePath == null)
            {
                var content = _locator.GetInstance<IContentLoader>().Get<IContent>(_PageGuid);
                _PagePath = _locator.GetInstance<UrlResolver>().GetVirtualPath(content);
            }

            if (_PagePath != null) // if we still dont have a page path, 
            {
                return RequestFilePath.Contains(_PagePath.VirtualPath);
            }
            else
            {
                return false;
            }
        }
    }
}
