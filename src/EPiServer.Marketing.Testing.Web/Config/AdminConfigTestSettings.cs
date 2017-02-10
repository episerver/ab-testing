using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
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

        internal static AdminConfigTestSettings _currentSettings;

       [ExcludeFromCodeCoverage]
        public static AdminConfigTestSettings Current
        {
            get
            {
                if (_currentSettings == null)
                {
                    var store = DynamicDataStoreFactory.Instance.GetStore(typeof(AdminConfigTestSettings));
                    _currentSettings = store.LoadAll<AdminConfigTestSettings>().OrderByDescending(x => x.Id.StoreId).FirstOrDefault() ?? new AdminConfigTestSettings();
                }

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
        }

        [ExcludeFromCodeCoverage]
        public void Save()
        {
            var store = DynamicDataStoreFactory.Instance.GetStore(typeof(AdminConfigTestSettings));
            store.Save(this);

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
