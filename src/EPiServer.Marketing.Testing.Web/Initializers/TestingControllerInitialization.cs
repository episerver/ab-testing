using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    class TestingControllerInitialization : IConfigurableModule
    {
        [ExcludeFromCodeCoverage]
        public void ConfigureContainer(ServiceConfigurationContext context) { }

        [ExcludeFromCodeCoverage]
        public void Uninitialize(InitializationEngine context) { }

        [ExcludeFromCodeCoverage]
        public void Initialize(InitializationEngine context)
        {
            RouteTable.Routes.MapHttpRoute(
                name: "EPiServerContentOptimization",
                routeTemplate: "api/episerver/Testing/{action}",
                defaults: new { controller = "Testing", action = "GetAllTests" }
            );
        }
    }
}
