using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using System.Linq;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Config
{
    /// <summary>
    /// Used to populate the dropdown in the admin config settings page for ABTesting
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class AvailableConfidenceLevels
    {
        public static IEnumerable<ConfidenceLevel> ConfidenceLevels = new List<ConfidenceLevel>
        {
            new ConfidenceLevel
            {
                Value = 99,
                Name = "99%"
            },
            new ConfidenceLevel
            {
                Value = 98,
                Name = "98%"
            },
            new ConfidenceLevel
            {
                Value = 95,
                Name = "95%"
            },
            new ConfidenceLevel
            {
                Value = 90,
                Name = "90%"
            }
        };
    }

    /// <summary>
    /// ABTest settings that are configurable via the admin config page
    /// </summary>
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class AdminConfigTestSettings : IDynamicData
    {
        public Identity Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Must be a positive number.")]
        public int TestDuration { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Must be between 1 and 100.")]
        public int ParticipationPercent { get; set; }

        public int ConfidenceLevel { get; set; }

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

    [ExcludeFromCodeCoverage]
    public class ConfidenceLevel
    {
        public string Name { get; set; }

        public int Value { get; set; }
    }

}
