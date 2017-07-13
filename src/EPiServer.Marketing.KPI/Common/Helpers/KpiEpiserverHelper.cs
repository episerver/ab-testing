using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace EPiServer.Marketing.KPI.Common.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(IKpiEpiserverHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiEpiserverHelper : IKpiEpiserverHelper
    {
        public string GetUrl(ContentReference contentReference)
        {
            return UrlResolver.Current.GetUrl(contentReference);
        }
    }
}
