using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Evaluator;
using EPiServer.ServiceLocation;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ExcludeFromCodeCoverage]
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class MarketingTestingInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var configuredTimeout = ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:CacheTimeoutInMinutes"]?.ToString();
            int.TryParse(configuredTimeout, out int timeout);

            context.Services.AddTransient<IContentLockEvaluator, ABTestLockEvaluator>();
            context.Services.AddSingleton<ITestManager, CachingTestManager>(
                serviceLocator =>
                    new CachingTestManager(
                        serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>(),
                        serviceLocator.GetInstance<DefaultMarketingTestingEvents>(),
                        new TestManager(),
                        LogManager.GetLogger(typeof(CachingTestManager)),
                        timeout < 10 ? timeout = 60 : timeout
                    ));

            context.Services.AddSingleton<ITestHandler, TestHandler>();
        }

        public void Initialize(InitializationEngine context) 
        {
            ServiceLocator.Current.GetInstance<ITestManager>();
            ServiceLocator.Current.GetInstance<ITestHandler>();
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}
