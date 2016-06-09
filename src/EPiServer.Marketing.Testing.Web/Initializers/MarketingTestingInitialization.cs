using System;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Web.MetadataExtender;
using EPiServer.Shell.ObjectEditing;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [InitializableModule]
    public class MarketingTestingInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {

        }

        public void Initialize(InitializationEngine context)
        {
            var metadataHandlerRegistry = context.Locate.Advanced.GetInstance<MetadataHandlerRegistry>();
            metadataHandlerRegistry.RegisterMetadataHandler(typeof(ContentData), context.Locate.Advanced.GetInstance<MarketingTestMetadataExtender>());

        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
