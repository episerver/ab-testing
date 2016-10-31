using System;
using System.Linq;
using System.Reflection;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Localization.XmlResources;

namespace EPiServer.Marketing.KPI.Commerce.Initializers
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class LanguageProviderInitializer : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            ProviderBasedLocalizationService ls = context.Locate.Advanced.GetInstance<LocalizationService>() as ProviderBasedLocalizationService;
            if (ls != null)
            {
                EmbeddedXmlLocalizationProviderInitializer localp = new EmbeddedXmlLocalizationProviderInitializer();

                // Gets embedded xml resources from the given assembly creating a new xml provider
                Assembly kpiAssembly = this.GetType().Assembly;
                var xmlProvider  = localp.GetInitializedProvider( 
                    kpiAssembly.GetType().Namespace, kpiAssembly );
                
                //Inserts the provider first in the provider list so that it is prioritized over default providers.
                ls.Providers.Insert(0, xmlProvider);
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
            ProviderBasedLocalizationService localizationService = context.Locate.Advanced.GetInstance<LocalizationService>() as ProviderBasedLocalizationService;
            Assembly kpiAssembly = this.GetType().Assembly;

            //Gets any provider that has the same name as the one initialized.
            LocalizationProvider lp = localizationService?.Providers.FirstOrDefault(
                p => p.Name.Equals(kpiAssembly.GetType().Namespace, StringComparison.Ordinal));
            if (lp != null)
            {
                //If found, remove it.
                localizationService.Providers.Remove(lp);
            }
        }
    }
}
