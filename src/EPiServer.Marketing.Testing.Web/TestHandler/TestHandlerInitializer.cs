using System;
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
        private readonly TestHandler _testHandler;
        private UrlResolver _pageRouteHelper;

        public TestHandlerInitializer()
        {
            _testHandler = new TestHandler();
        }

        public void Initialize(InitializationEngine context)
        {
            _testHandler.Initialize();
        }

        public void Uninitialize(InitializationEngine context)
        {
            _testHandler.Uninitialize();
        }

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.BeginRequest += BeginRequest;
            application.EndRequest += EndRequest;
        }

        private void BeginRequest(object sender, EventArgs e)
        {
            _pageRouteHelper = ServiceLocator.Current.GetInstance<UrlResolver>();

            //Convert URL to content and store it in the current requests items collection
            //This collection is volatile and will be cleared at the end of the session, preventing threading issues.
            HttpContext.Current.Items["CurrentPage"] = _pageRouteHelper.Route(new UrlBuilder(HttpContext.Current.Request.Url));
        }

        private void EndRequest(object sender, EventArgs e)
        {
            _testHandler.ProcessedContentList.Clear();
        }


      

    }
}
