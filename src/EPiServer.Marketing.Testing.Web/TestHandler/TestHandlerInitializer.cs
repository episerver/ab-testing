using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.ClientKPI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace EPiServer.Marketing.Testing.Web
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class TestHandlerInitializer : IInitializableHttpModule
    {
        private TestHandler _testHandler;
        private IClientKpiInjector _clientKpiInjector;
        private readonly ILogger _logger;

        [ExcludeFromCodeCoverage]
        public TestHandlerInitializer(ILogger logger)
        {
            _logger = logger;
        }

        [ExcludeFromCodeCoverage]
        public void Initialize(InitializationEngine context)
        {
            _testHandler = new TestHandler();
            _clientKpiInjector = new ClientKpiInjector();
        }

        [ExcludeFromCodeCoverage]
        public void InitializeHttpEvents(HttpApplication application)
        {
            // We are not actually doing in anything in begin and end request 
            // anymore however leaving this here in case we do. 
            //  application.BeginRequest += BeginRequest;
            //application.EndRequest += EndRequest;
            application.PostReleaseRequestState += onPostReleaseRequestState;
        }

        private void onPostReleaseRequestState(object sender, EventArgs e)
        {
            try
            {
                _clientKpiInjector.AppendClientKpiScript();
            }
            catch(Exception ex)
            {
                // Don't crash the site on an A/B failure. Make note of the error
                // and allow the site to carry on.

                _logger.Error("An error occurred while attempting to append the client KPI script.", ex);
            }
        }

        //Interface Requirement but not used.
        [ExcludeFromCodeCoverage]
        public void Uninitialize(InitializationEngine context) { }
    }
}
