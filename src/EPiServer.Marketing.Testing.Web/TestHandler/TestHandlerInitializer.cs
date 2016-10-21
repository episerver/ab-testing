using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace EPiServer.Marketing.Testing.Web
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class TestHandlerInitializer : IInitializableHttpModule
    {
        private TestHandler _testHandler;

        [ExcludeFromCodeCoverage]
        public TestHandlerInitializer()
        {
        }

        [ExcludeFromCodeCoverage]
        public void Initialize(InitializationEngine context)
        {
            _testHandler = new TestHandler();
        }

        [ExcludeFromCodeCoverage]
        public void InitializeHttpEvents(HttpApplication application)
        {
            application.BeginRequest += BeginRequest;
            application.EndRequest += EndRequest;
        }

        [ExcludeFromCodeCoverage]
        private void BeginRequest(object sender, EventArgs e)
        {
            // Get the page associate with this request once
            // and store in the request so we can use it later
            var pageHelper = ServiceLocator.Current.GetInstance<EPiServer.Web.Routing.PageRouteHelper>();
            HttpContext.Current.Items["CurrentPage"] = pageHelper.Page;
        }

        [ExcludeFromCodeCoverage]
        private void EndRequest(object sender, EventArgs e)
        {
        }

        //Interface Requirement but not used.
        [ExcludeFromCodeCoverage]
        public void Uninitialize(InitializationEngine context) { }
    }
}
