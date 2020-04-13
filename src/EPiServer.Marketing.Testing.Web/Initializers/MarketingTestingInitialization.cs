using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Evaluator;
using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ExcludeFromCodeCoverage]
    [InitializableModule]
    public class MarketingTestingInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IContentLockEvaluator, ABTestLockEvaluator>();

            context.Services.AddSingleton<ITestManager, CachingTestManager>(
                serviceLocator =>
                    new CachingTestManager(
                        new MemoryCache("Episerver.Marketing.Testing"),
                        new RemoteCacheSignal(
                            serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>(),
                            LogManager.GetLogger(),
                            "epi/marketing/testing/cache",
                            TimeSpan.FromMilliseconds(100)
                        ),
                        serviceLocator.GetInstance<DefaultMarketingTestingEvents>(),
                        new TestManager()
                    ));

            context.Services.AddSingleton<IConfigurationMonitor, ConfigurationMonitor>(
            serviceLocator =>
                new ConfigurationMonitor(serviceLocator,
                    new RemoteCacheSignal(
                        serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>(),
                        LogManager.GetLogger(),
                        "epi/marketing/testing/configuration",
                        TimeSpan.FromMilliseconds(500)
                    )));

            context.Services.AddSingleton<ITestHandler, TestHandler>();
            context.Services.AddSingleton<IFeatureEnabler, FeatureEnabler>(
                serviceLocator => new FeatureEnabler(serviceLocator));
        }

        public void Initialize(InitializationEngine context) { }

        public void Uninitialize(InitializationEngine context) { }
    }
}
