using System;

namespace EPiServer.Marketing.Testing.Data
{
    public class KeyPerformanceIndicator
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the test this conversion is associated with.
        /// </summary>
        public Guid? TestId { get; set; }

        /// <summary>
        /// Id of the kpi.
        /// </summary>
        public Guid KeyPerformanceIndicatorId { get; set; }
    }
}
