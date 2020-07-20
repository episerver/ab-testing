using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Evaluator;
using EPiServer.ServiceLocation;
using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ExcludeFromCodeCoverage]
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class MarketingTestingInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IContentLockEvaluator, ABTestLockEvaluator>();

            int.TryParse(ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:ConfigurationMonitorSeconds"]?.ToString(), out var configurationMonitorValue);
            int.TryParse(ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:TestMonitorSeconds"]?.ToString(), out var testMonitorValue);

            context.Services.AddSingleton<ITestManager, CachingTestManager>(
                serviceLocator =>
                    new CachingTestManager(
                        new MemoryCache("Episerver.Marketing.Testing"),
                        new RemoteCacheSignal(
                            serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>(),
                            LogManager.GetLogger(),
                            "epi/marketing/testing/cache",
                            TimeSpan.FromSeconds(testMonitorValue > 0 ? testMonitorValue : 1)
                        ),
                        new RemoteCacheSignal(
                        ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>(),
                        LogManager.GetLogger(),
                        "epi/marketing/testing/configuration",
                        TimeSpan.FromSeconds(configurationMonitorValue > 0 ? configurationMonitorValue : 60)
                    ),
                        serviceLocator.GetInstance<DefaultMarketingTestingEvents>(),
                        new TestManager()
                    ));

            
            context.Services.AddSingleton<IConfigurationMonitor, ConfigurationMonitor>( 
                serviceLocator =>
                    new ConfigurationMonitor(serviceLocator, new RemoteCacheSignal(
                        ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>(),
                        LogManager.GetLogger(),
                        "epi/marketing/testing/configuration",
                        TimeSpan.FromSeconds(configurationMonitorValue > 0 ? configurationMonitorValue : 60)
                    )));

            context.Services.AddSingleton<ITestHandler, TestHandler>();
            context.Services.AddSingleton<IFeatureEnabler, FeatureEnabler>(
                serviceLocator => new FeatureEnabler(serviceLocator));
        }

        public void Initialize(InitializationEngine context) 
        {
            ServiceLocator.Current.GetInstance<ITestManager>();
            ServiceLocator.Current.GetInstance<ITestHandler>();
            ServiceLocator.Current.GetInstance<IFeatureEnabler>();
            ServiceLocator.Current.GetInstance<IConfigurationMonitor>();
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}
