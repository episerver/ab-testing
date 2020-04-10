using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.ClientKPI;
using EPiServer.Marketing.Testing.Web.Initializers;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    [ModuleDependency(typeof(MarketingTestingInitialization))]
    public class TestHandlerInitializer : IInitializableHttpModule
    {
        private IClientKpiInjector _clientKpiInjector;
        private ILogger _logger;

        [ExcludeFromCodeCoverage]
        public TestHandlerInitializer()
        {
            _logger = LogManager.GetLogger();
        }

        [ExcludeFromCodeCoverage]
        public void Initialize(InitializationEngine context)
        {
            ServiceLocator.Current.GetInstance<ITestHandler>();
            ServiceLocator.Current.GetInstance<IFeatureEnabler>();

            _clientKpiInjector = new ClientKpiInjector();
            _logger = LogManager.GetLogger();
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
                _logger.Error("An error occurred while injecting client KPI script.", ex);
            }
        }

        //Interface Requirement but not used.
        [ExcludeFromCodeCoverage]
        public void Uninitialize(InitializationEngine context) { }
    }
}
