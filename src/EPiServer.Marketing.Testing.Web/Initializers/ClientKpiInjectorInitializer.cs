using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.ClientKPI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    [ModuleDependency(typeof(MarketingTestingInitialization))]
    public class ClientKpiInjectorInitializer : IInitializableHttpModule
    {
        private IClientKpiInjector _clientKpiInjector;
        private ILogger _logger;

        [ExcludeFromCodeCoverage]
        public ClientKpiInjectorInitializer()
        {
            _logger = LogManager.GetLogger();
        }

        [ExcludeFromCodeCoverage]
        public void Initialize(InitializationEngine context)
        {
            _clientKpiInjector = new ClientKpiInjector();
            _logger = LogManager.GetLogger();
        }

        [ExcludeFromCodeCoverage]
        public void InitializeHttpEvents(HttpApplication application)
        {
            application.PostReleaseRequestState += onPostReleaseRequestState;
        }

        private void onPostReleaseRequestState(object sender, EventArgs e)
        {
            try
            {
                _clientKpiInjector.AppendClientKpiScript();
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while injecting client KPI script.", ex);
            }
        }

        //Interface Requirement but not used.
        [ExcludeFromCodeCoverage]
        public void Uninitialize(InitializationEngine context) { }
    }
}
