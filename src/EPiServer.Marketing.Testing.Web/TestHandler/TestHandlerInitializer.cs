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

        }

       

        private void EndRequest(object sender, EventArgs e)
        {
           
            _testHandler.ProcessedContentList.Clear();
        }


      

    }
}
