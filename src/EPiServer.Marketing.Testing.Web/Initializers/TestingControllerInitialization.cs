using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Web.Helpers;
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
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "EPiServerContentOptimization",
                routeTemplate: "api/episerver/Testing/{action}",
                defaults: new { controller = "Testing", action = "GetAllTests" }
            );
        }
    }
}
