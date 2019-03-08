using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Evaluator;
using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ExcludeFromCodeCoverage]
    [InitializableModule]
    public class MarketingTestingInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context) {
            context.Services.AddTransient<IContentLockEvaluator, ABTestLockEvaluator>();

            context.Services.AddSingleton<ITestManager, CachingTestManager>(
                serviceLocator =>
                    new CachingTestManager(
                        serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>(),
                        serviceLocator.GetInstance<IContentLoader>(),
                        new TestManager()
                    )
            );
        }

        public void Initialize(InitializationEngine context){ }

        public void Uninitialize(InitializationEngine context) { }
    }
}
