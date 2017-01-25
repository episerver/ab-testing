using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EPiServer.Marketing.KPI.Commerce.Config
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class CommerceKpiSettings : IDynamicData
    {
        public Identity Id { get; set; }      

        public IMarket PreferredMarket { get; set; }

        internal static CommerceKpiSettings _currentSettings;

        [ExcludeFromCodeCoverage]
        public static CommerceKpiSettings Current
        {
            get
            {
                if (_currentSettings == null)
                {
                    _currentSettings = new CommerceKpiSettings();
                    var kpiManager = ServiceLocator.Current.GetInstance<IKpiManager>();
                    var marketService = ServiceLocator.Current.GetInstance<IMarketService>();

                    var preferredMarket = kpiManager.GetCommerceSettings();

                    _currentSettings.PreferredMarket = !string.IsNullOrEmpty(preferredMarket.PreferredMarketValue) ? 
                        _currentSettings.PreferredMarket = marketService.GetMarket(preferredMarket.PreferredMarketValue)
                        :
                        _currentSettings.PreferredMarket = marketService.GetMarket(MarketId.Default.Value);
                    
                }

                return _currentSettings;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommerceKpiSettings"/> class.
        /// </summary>
        public CommerceKpiSettings()
        {
            var marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            PreferredMarket = marketService.GetMarket(MarketId.Default.Value); 
        }

        [ExcludeFromCodeCoverage]
        public void Save()
        {
            var kpiManager = ServiceLocator.Current.GetInstance<IKpiManager>();
            var settingsToSave = new CommerceData();
            settingsToSave.PreferredMarketValue = PreferredMarket.MarketId.Value;
            settingsToSave.preferredFormat = PreferredMarket.DefaultCurrency.Format;
            kpiManager.SaveCommerceSettings(settingsToSave);
            
            _currentSettings = this;
        }

        /// <summary>
        /// Clears setting values that being stored in memory.
        /// </summary>
        public void Reset()
        {
            _currentSettings = null;
        }      
    }   
}
