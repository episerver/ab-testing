using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;

namespace EPiServer.Marketing.Testing.Web.Config
{
    /// <summary>
    /// ABTest settings that are configurable via the admin config page
    /// </summary>
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class AdminConfigTestSettings : IDynamicData
    {
        public Identity Id { get; set; }

        [Required]
        [Range(1, 365, ErrorMessage = "Must be a positive number between 1 and 365.")]
        public int TestDuration { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Must be between 1 and 100.")]
        public int ParticipationPercent { get; set; }

        public int ConfidenceLevel { get; set; }

        public bool AutoPublishWinner { get; set; }

        /// <summary>
        /// Hardcoded limit is set to 5.  This is not part of the UI, but can be added if users request it.
        /// </summary>
        [Range(1, 20, ErrorMessage = "Must be a positive number between 1 and 20")]
        public int KpiLimit { get; set; }

        [StringLength(1, ErrorMessage = "Must be a single character.")]
        public string CookieDelimeter { get; set; }

        public bool IsEnabled { get; set; }

        public string PreviewStyleOverride { get; set; }
        

        internal static AdminConfigTestSettings _currentSettings;
        internal static DynamicDataStoreFactory _factory;
        internal IServiceLocator _serviceLocator;

        /// <summary>
        /// Resets the config and forces a re-read of the settings from the db.
        /// </summary>
        /// <returns></returns>
        public static void Reset()
        {
            _currentSettings = null;
        }

        /// <summary>
        /// Gets the current config
        /// </summary>
        public static AdminConfigTestSettings Current
        {
            get
            {
                if (_currentSettings == null)
                {
                    if (_factory == null)
                    {
                        _factory = DynamicDataStoreFactory.Instance;
                    }
                    var store = _factory.GetStore(typeof(AdminConfigTestSettings));
                    _currentSettings = store.LoadAll<AdminConfigTestSettings>().OrderByDescending(x => x.Id.StoreId).FirstOrDefault() ?? new AdminConfigTestSettings();
                    if (_currentSettings.CookieDelimeter == null)
                    {
                        _currentSettings.CookieDelimeter = "_";
                    }
                }

                _currentSettings.PreviewStyleOverride = ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:PreviewStyleOverride"]?.ToString();
                return _currentSettings;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminConfigTestSettings"/> class.
        /// </summary>
        public AdminConfigTestSettings()
        {
            TestDuration = 30;
            ParticipationPercent = 10;
            ConfidenceLevel = 95;
            AutoPublishWinner = false;
            KpiLimit = 5;
            CookieDelimeter = "_";
            IsEnabled = true;
            PreviewStyleOverride = "";
        }

        public void Save()
        {
            var store = DynamicDataStoreFactory.Instance.GetStore(typeof(AdminConfigTestSettings));
            store.Save(this);

            _currentSettings = this;
            var repo = (_serviceLocator == null ? ServiceLocator.Current : _serviceLocator).GetInstance<IMarketingTestingWebRepository>();
            repo.ConfigurationChanged();
        }
    }
}
