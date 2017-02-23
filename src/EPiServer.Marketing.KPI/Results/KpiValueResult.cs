using System;

namespace EPiServer.Marketing.KPI.Results
{
    /// <summary>
    /// Value result for kpi's.  This is meant for any non-financial numbers.
    /// </summary>
    public class KpiValueResult : IKpiResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <inheritdoc />
        public bool HasConverted { get; set; }

        /// <summary>
        /// The value that was calculated as part of the kpi's evaluate method.
        /// </summary>
        public double Value { get; set; }

    }
}
