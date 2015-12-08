using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace EPiServerSite4.Init
{
    [InitializableModule]
    public class MultivariateTestRouteInitializer : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {

            MapMultivariateTestRoute(RouteTable.Routes);

        }

        private static void MapMultivariateTestRoute(RouteCollection routes)
        {
            routes.MapRoute(name: "AB API Testing",
               url: "MultivariateApiTesting/{action}/{state}",
               defaults: new { controller = "MultivariateApiTesting", action = "Index", state = UrlParameter.Optional });

        }

        public void Uninitialize(InitializationEngine context)
        {
            throw new System.NotImplementedException();
        }
    }
}