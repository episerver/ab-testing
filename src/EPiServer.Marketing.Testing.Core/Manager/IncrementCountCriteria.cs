using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using System;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Used to sepcify that type of count to increment.
    /// </summary>
    public class IncrementCountCriteria
    {
        /// <summary>
        /// ID of a test to save the result to.
        /// </summary>
        public Guid testId { get; set; }
        /// <summary>
        /// Version of the variant that should be updated.
        /// </summary>
        public int itemVersion { get; set; }
        /// <summary>
        /// Saves a KPI result.  The result is appended to the list of results for a given variant version for a test for both historical and statistical calculations.
        /// </summary>
        public CountType resultType { get; set; } = CountType.View;
        /// <summary>
        /// ID of the KPI count to increment.
        /// </summary>
        public Guid kpiId { get; set; } = default(Guid);
        /// <summary>
        /// Boolean stating whether the result should be saved asynchronously or not.
        /// </summary>
        public bool asynch { get; set; } = true;
        /// <summary>
        /// A unique identifier to throttle overly agressive clients when performing a conversion update.
        /// </summary>
        public string clientId { get; set; } = null;
    }
}
