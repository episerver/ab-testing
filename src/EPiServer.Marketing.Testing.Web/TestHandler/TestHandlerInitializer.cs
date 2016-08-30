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
        private UrlResolver _pageRouteHelper;

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
            _pageRouteHelper = ServiceLocator.Current.GetInstance<UrlResolver>();

            //Convert URL to content and stores it in the current requests items collection
            //This collection is volatile and will be cleared at the end of the session, preventing threading issues.
            HttpContext.Current.Items["CurrentPage"] = _pageRouteHelper.Route(new UrlBuilder(HttpContext.Current.Request.Url));

            // TestHandler needs to know when the request starts, loadContent will clear 
            // this flag after it does its first time processing.
            HttpContext.Current.Items[TestHandler.ABTestFirstRequestFlag] = true;
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
