using System;
using System.Web;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace EPiServer.Marketing.Testing.Web
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class TestHandlerInitializer : IInitializableHttpModule
    {
        private readonly TestHandler _testHandler;

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
            application.EndRequest += EndRequest;
            application.BeginRequest += BeginRequest;

        }

        private void BeginRequest(object sender, EventArgs e)
        {
            if (IsRequestToRoot())
            {
                _testHandler.SwapDisabled = true;
            }
            

        }

        private void EndRequest(object sender, EventArgs e)
        {
            if (IsRequestToRoot())
            {
                _testHandler.SwapDisabled = false;
            }
            _testHandler.ProcessedContentList.Clear();
        }


        private bool IsRequestToRoot()
        {
            return
                HttpContext.Current.Request.Url.AbsoluteUri.ToLower()
                    .Contains(EPiServer.Shell.Paths.ProtectedRootPath.ToLower());


        }

    }
}
