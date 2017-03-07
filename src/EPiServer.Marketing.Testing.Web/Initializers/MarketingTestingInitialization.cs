using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Web.MetadataExtender;
using EPiServer.Shell.ObjectEditing;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Evaluator;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ExcludeFromCodeCoverage]
    [InitializableModule]
    public class MarketingTestingInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context) {
            context.Services.AddTransient<IContentLockEvaluator, ABTestLockEvaluator>();
        }

        public void Initialize(InitializationEngine context)
        {
            var metadataHandlerRegistry = context.Locate.Advanced.GetInstance<MetadataHandlerRegistry>();
            metadataHandlerRegistry.RegisterMetadataHandler(typeof(ContentData), context.Locate.Advanced.GetInstance<MarketingTestMetadataExtender>());
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}
