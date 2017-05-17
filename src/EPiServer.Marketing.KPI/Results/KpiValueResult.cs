using System;

namespace EPiServer.Marketing.KPI.Results
{
    /// <summary>
    /// Value result for KPIs.  This is meant for any non-financial numbers.
    /// </summary>
    public class KpiValueResult : IKpiResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <inheritdoc />
        public bool HasConverted { get; set; }

        /// <summary>
        /// The value that was calculated as part of the KPIs evaluate method.
        /// </summary>
        public double Value { get; set; }

    }
}
