using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Web.Config
{
    /// <summary>
    /// Used to populate the dropdown in the admin config settings page for ABTesting
    /// </summary>
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
    public class AdminConfigTestSettings
    {
        public int TestDuration { get; set; }

        public int ParticipationPercentage { get; set; }

        public int ConfidenceLevel { get; set; }
    }

    public class ConfidenceLevel
    {
        public string Name { get; set; }

        public int Value { get; set; }
    }

}
